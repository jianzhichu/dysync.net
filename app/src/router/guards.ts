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

// ========== 新增：移动端检测核心函数 ==========
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

// ========== 新增：移动端跳转守卫（已集成登录状态判断） ==========
const MobileRedirectGuard: NavigationGuard = function (to, from, next) {
  // 1. 排除/mobile路由本身，避免无限循环
  if (to.path === '/mobile') {
    hasRedirectedToMobile = true;
    next();
    return;
  }

  // 2. 排除/login路由，避免登录页被移动端跳转逻辑覆盖
  if (to.path === '/login') {
    next();
    return;
  }

  // 3. 检测是否为移动端
  if (isMobile() && !hasRedirectedToMobile) {
    // 4. 核心判断：检查登录状态
    const isAuthorized = http.checkAuthorization();
    if (!isAuthorized) {
      // 未登录：优先跳转到登录页
      hasRedirectedToMobile = false; // 重置标记，不影响后续登录后的跳转
      next('/login');
    } else {
      // 已登录：跳转到移动端路由
      hasRedirectedToMobile = true;
      next({ path: '/mobile' });
    }
  } else {
    // 非移动端/已跳转：重置标记并执行原有逻辑
    hasRedirectedToMobile = false;
    next();
  }
};

// ========== 原有守卫逻辑（无修改） ==========
const loginGuard: NavigationGuard = function (to, from, next) {
  // 补充next参数，保证守卫链正常执行
  if (!http.checkAuthorization() && !/^\/(init|login|home|mobile)?$/.test(to.fullPath)) {
    console.log(to.fullPath)
    const account = useAccountStore();
    account.setLogged(false);
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
  // 补充next参数
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
    next(); // 补充next参数
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
    next(); // 补充next参数
  },
};

// 404 not found
const NotFoundGuard: NaviGuard = {
  before(to, from, next) {
    const { loading } = useMenuStore();
    if (to.meta._is404Page && loading) {
      to.params.loading = true as any;
    }
    next(); // 补充next参数
  },
};

// ========== 页面刷新时的移动端检测（已集成登录状态判断） ==========
window.addEventListener('load', () => {
  if (isMobile() && window.location.pathname !== '/mobile') {
    // 检查登录状态：未登录则跳登录，已登录则跳移动端
    const isAuthorized = http.checkAuthorization();
    if (!isAuthorized) {
      if (window.location.pathname !== '/login') {
        router.push('/login').catch(err => {
          if (!err.message.includes('NavigationDuplicated')) {
            console.error('刷新时跳转登录页失败：', err);
          }
        });
      }
    } else {
      router.push('/mobile').catch(err => {
        if (!err.message.includes('NavigationDuplicated')) {
          console.error('刷新时跳转移动端路由失败：', err);
        }
      });
    }
  }
});

export default {
  // 把MobileRedirectGuard放在最前面，优先执行移动端检测
  before: [ProgressGuard.before, MobileRedirectGuard, loginGuard, AuthGuard.before, ForbiddenGuard.before, NotFoundGuard.before],
  after: [ProgressGuard.after],
};