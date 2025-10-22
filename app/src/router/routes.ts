import { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [

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
  },
  // {
  //   path: '/',
  //   name: 'init',
  //   redirect: '/init',
  //   meta: {
  //     title: '初始化',
  //     renderMenu: false,
  //     icon: 'CreditCardOutlined',
  //   },
  //   children: null,
  //   component: () => import('@/pages/init'),
  // },
  {
    path: '/front',
    name: '前端',
    meta: {
      renderMenu: false,
    },
    component: () => import('@/components/layout/FrontView.vue'),
    children: [
      {
        path: '/login',
        name: '登录',
        meta: {
          icon: 'LoginOutlined',
          view: 'blank',
          target: '_blank',
          cacheable: false,
        },
        component: () => import('@/pages/login'),
      },

      // {
      //   path: '/init',
      //   name: '初始化',
      //   meta: {
      //     icon: 'LoginOutlined',
      //     view: 'blank',
      //     target: '_blank',
      //     cacheable: false,
      //   },
      //   component: () => import('@/pages/init'),
      // },
    ],
  },
  {
    path: '/403',
    name: '403',
    props: true,
    meta: {
      renderMenu: false,
    },
    component: () => import('@/pages/Exp403.vue'),
  },

  // {
  //   id: 1,
  //   name: '解析记录',
  //   title: '解析记录',
  //   icon: 'DashboardOutlined',
  //   badge: '',
  //   target: '_self',
  //   path: '/workplace',
  //   component: () => import('@/pages/workplace/Records.vue'),
  //   renderMenu: true,
  //   parent: null,
  //   permission: null,
  //   cacheable: false,
  // },
  {
    path: '/dashboard',
    name: '数据看板',
    meta: {
      icon: 'RadarChartOutlined',
      view: 'self',
      target: '_self',
      renderMenu: true,
      cacheable: false,
    },
    component: () => import('@/pages/workplace/statics.vue'),
  },
  {
    path: '/workplace',
    name: '同步记录',
    meta: {
      icon: 'SwapOutlined',
      view: 'self',
      target: '_self',
      renderMenu: true,
      cacheable: false,
    },
    component: () => import('@/pages/workplace/Workplace.vue'),
  },
  {
    path: '/cok',
    name: '抖音授权',
    meta: {
      icon: 'SafetyCertificateOutlined',
      view: 'self',
      target: '_self',
      renderMenu: true,
      cacheable: false,
    },
    component: () => import('@/pages/cok/Table.vue'),
  },
  {
    path: '/set',
    name: '系统配置',
    meta: {
      icon: 'SettingOutlined',
      view: 'self',
      target: '_self',
      renderMenu: true,
      cacheable: false,
    },
    component: () => import('@/pages/set/AppSet.vue'),
  },
  // {
  //   // id: 3,
  //   name: '系统日志',
  //   // title: '系统日志',
  //   icon: 'UnorderedListOutlined',
  //   badge: '',
  //   target: '_self',
  //   path: '/logs',
  //   component: () => import('@/pages/mylogs/MyLogs.vue'),
  //   renderMenu: true,
  //   parent: null,
  //   permission: null,
  //   cacheable: false,
  // },

  {
    path: '/logs',
    name: '系统日志',
    meta: {
      icon: 'UnorderedListOutlined',
      view: 'self',
      target: '_self',
      renderMenu: true,
      cacheable: false,
    },
    component: () => import('@/pages/mylogs/MyLogs.vue'),
  },
  {
    path: '/:pathMatch(.*)*',
    name: '404',
    props: true,
    meta: {
      icon: 'CreditCardOutlined',
      renderMenu: false,
      cacheable: false,
      _is404Page: true,
    },
    component: () => import('@/pages/Exp404.vue'),
  },
];

export default routes;
