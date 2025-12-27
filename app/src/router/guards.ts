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

const loginGuard: NavigationGuard = function (to, from) {
  // console.log('Authorization', http.checkAuthorization())
  const account = useAccountStore();
  if (!http.checkAuthorization() && !/^\/(init|login|home|mobile)?$/.test(to.fullPath)) {
    console.log(123)
    console.log(to.fullPath)
    account.setLogged(false)
    return '/login';
  } else {
  }

};

const dynamicinitRoute =
{
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


const InitGuard: NavigationGuard = function (to, from) {

  if (to.fullPath != '/login') {
    if (!router.hasRoute('login')) {
      router.addRoute(dynamicinitRoute)
    }
    router.push('/login')
  }
};


// 进度条
const ProgressGuard: NaviGuard = {
  before(to, from) {
    NProgress.start();
  },
  after(to, from) {
    NProgress.done();
  },
};

const AuthGuard: NaviGuard = {
  before(to, from) {
    const { hasAuthority } = useAuthStore();
    if (to.meta?.permission && !hasAuthority(to.meta?.permission)) {
      return { name: '403', query: { permission: to.meta.permission, path: to.fullPath } };
    }
  },
};

const ForbiddenGuard: NaviGuard = {
  before(to) {
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
  },
};

// 404 not found
const NotFoundGuard: NaviGuard = {
  before(to, from) {
    const { loading } = useMenuStore();
    if (to.meta._is404Page && loading) {
      to.params.loading = true as any;
    }
  },
};

export default {
  // before: [ProgressGuard.before, InitGuard, loginGuard, AuthGuard.before, ForbiddenGuard.before, NotFoundGuard.before],
  before: [ProgressGuard.before, loginGuard, AuthGuard.before, ForbiddenGuard.before, NotFoundGuard.before],
  after: [ProgressGuard.after],
};
