import { NavigationGuard, NavigationHookAfter } from 'vue-router';
import http from '@/store/http';
import { useAccountStore, useMenuStore, useApiStore } from '@/store';
import { useAuthStore } from '@/plugins';
import NProgress from 'nprogress';
import 'nprogress/nprogress.css';
import router from '@/router';

NProgress.configure({ showSpinner: false });

interface NaviGuard {
  before?: NavigationGuard;
  after?: NavigationHookAfter;
}

/**
 * 检测是否为移动端设备（UA + 屏幕宽度双检测）
 */
const isMobile = (): boolean => {
  const userAgent = navigator.userAgent.toLowerCase();
  const mobileUaReg = /iphone|android|ipad|ipod|mobile|wap|symbian|windows ce|blackberry|webos|ucbrowser/i;
  const isSmallScreen = window.innerWidth < 768;
  return mobileUaReg.test(userAgent) || isSmallScreen;
};

// 标记是否已跳转到移动端路由，防止无限循环
let hasRedirectedToMobile = false;
// 新增：标记是否已从/mobile跳转到/dashboard，防止无限循环
let hasRedirectedToDashboard = false;
// 优化后的移动端跳转守卫（登录优先）
const MobileRedirectGuard: NavigationGuard = function (to, from, next) {
  // 1. 排除移动端路由和登录页，避免逻辑干扰
  const isMobileRoute = to.path === '/mobile';
  const isLoginRoute = to.path === '/login';
  // 新增：检测当前是否为非移动端设备
  const isNonMobile = !isMobile();

  // 【新增核心逻辑】：当前路由是/mobile，但不是移动端设备，跳转到/dashboard
  if (isMobileRoute && isNonMobile) {
    if (!hasRedirectedToDashboard) {
      hasRedirectedToDashboard = true;
      // 重置移动端跳转标记，避免后续干扰
      hasRedirectedToMobile = false;
      next({ path: '/dashboard' });
    } else {
      next();
    }
    return;
  }

  if (isMobileRoute) {
    hasRedirectedToMobile = true;
    // 重置dashboard跳转标记，避免后续干扰
    hasRedirectedToDashboard = false;
    next();
    return;
  }

  if (isLoginRoute) {
    // 登录页无需移动端跳转，直接放行
    hasRedirectedToMobile = false; // 重置标记，登录后可正常跳转移动端
    hasRedirectedToDashboard = false; // 重置dashboard跳转标记
    next();
    return;
  }

  // 2. 移动端检测
  if (isMobile()) {
    const isAuthorized = http.checkAuthorization();
    if (!isAuthorized) {
      // 未登录：优先跳登录（保持原有优先级）
      next('/login');
    } else {
      // 已登录：跳移动端（防止重复跳转）
      if (!hasRedirectedToMobile) {
        hasRedirectedToMobile = true;
        hasRedirectedToDashboard = false; // 重置dashboard跳转标记
        next({ path: '/mobile' });
      } else {
        next();
      }
    }
  } else {
    // 非移动端：重置所有标记，不影响后续操作
    hasRedirectedToMobile = false;
    hasRedirectedToDashboard = false;
    next();
  }
};
const loginGuard: NavigationGuard = function (to, from, next) {
  if (!http.checkAuthorization() && !/^\/(init|login|home|mobile)?$/.test(to.fullPath)) {
    console.log(to.fullPath)
    const account = useAccountStore();
    account.setLogged(false);
    // 重置dashboard跳转标记
    hasRedirectedToDashboard = false;
    next('/login');
  } else {
    next();
  }
};
const dynamicinitRoute = {
  path: '/',
  name: 'login',
  redirect: '/login',
  meta: {
    title: '登录',
    renderMenu: false,
    icon: 'CreditCardOutlined',
  },
  children: null,
  component: () => import('@/pages/login'),
};

const InitGuard: NavigationGuard = function (to, from, next) {
  if (to.fullPath != '/login') {
    if (!router.hasRoute('login')) {
      router.addRoute(dynamicinitRoute);
    }
    next('/login');
  } else {
    next();
  }
};

// 进度条
const ProgressGuard: NaviGuard = {
  before(to, from, next) {
    NProgress.start();
    next();
  },
  after(to, from) {
    NProgress.done();
  },
};

const AuthGuard: NaviGuard = {
  before(to, from, next) {
    const { hasAuthority } = useAuthStore();
    if (to.meta?.permission && !hasAuthority(to.meta?.permission)) {
      next({ name: '403', query: { permission: to.meta.permission, path: to.fullPath } });
    } else {
      next();
    }
  },
};

const ForbiddenGuard: NaviGuard = {
  before(to, from, next) {
    if (to.name === '403' && (to.query.permission || to.query.path)) {
      to.fullPath = to.fullPath
        .replace(/permission=[^&=]*&?/, '')
        .replace(/&?path=[^&=]*&?/, '')
        .replace(/\?$/, '');
      to.params.permission = to.query.permission;
      to.params.path = to.query.path;
      delete to.query.permission;
      delete to.query.path;
    }
    next();
  },
};

// 404 not found
const NotFoundGuard: NaviGuard = {
  before(to, from, next) {
    const { loading } = useMenuStore();
    if (to.meta._is404Page && loading) {
      to.params.loading = true as any;
    }
    next();
  },
};

// 优化后的页面刷新移动端检测（登录优先）
window.addEventListener('load', () => {
  const currentPath = window.location.pathname;
  const isNonMobile = !isMobile();

  // 【新增】：刷新后如果是/mobile路由且非移动端，跳转到/dashboard
  if (currentPath === '/mobile' && isNonMobile) {
    router.push('/dashboard').catch(err => {
      if (!err.message.includes('NavigationDuplicated')) {
        console.error('刷新时从/mobile跳转到/dashboard失败：', err);
      }
    });
    return;
  }

  if (isMobile() && currentPath !== '/mobile') {
    const isAuthorized = http.checkAuthorization();
    if (!isAuthorized) {
      // 未登录：跳登录（避免重复跳转）
      if (currentPath !== '/login') {
        router.push('/login').catch(err => {
          if (!err.message.includes('NavigationDuplicated')) {
            console.error('刷新时跳转登录页失败：', err);
          }
        });
      }
    } else {
      // 已登录：跳移动端
      router.push('/mobile').catch(err => {
        if (!err.message.includes('NavigationDuplicated')) {
          console.error('刷新时跳转移动端路由失败：', err);
        }
      });
    }
  }
});

export default {
  before: [ProgressGuard.before, MobileRedirectGuard, loginGuard, AuthGuard.before, ForbiddenGuard.before, NotFoundGuard.before],
  after: [ProgressGuard.after],
};