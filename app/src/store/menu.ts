import { defineStore, storeToRefs } from 'pinia';
import http from './http';
import { ref, watch } from 'vue';
import { Response } from '@/types';
import { RouteOption } from '@/router/interface';
import { addRoutes, removeRoute } from '@/router/dynamicRoutes';
import { useSettingStore } from './setting';
import { RouteRecordRaw, RouteMeta } from 'vue-router';
import { useAuthStore } from '@/plugins';
import router from '@/router';

export interface MenuProps {
  id?: number;
  name: string;
  path: string;
  title?: string;
  icon?: string;
  badge?: number | string;
  target?: '_self' | '_blank';
  link?: string;
  component: string;
  renderMenu?: boolean;
  permission?: string;
  parent?: string;
  children?: MenuProps[];
  cacheable?: boolean;
  view?: string;
}

/**
 * 过滤菜单
 * @param routes
 * @param parentPermission
 */
function doMenuFilter(routes: Readonly<RouteRecordRaw[]>, parentPermission?: string) {
  const { hasAuthority } = useAuthStore();

  const setCache = (meta: RouteMeta) => {
    meta._cache = {
      renderMenu: meta.renderMenu,
    };
  };

  routes.forEach((route) => {
    const required = route.meta?.permission ?? parentPermission;
    // if (route.meta?.renderMenu === undefined && required) {
    if (required) {
      route.meta = route.meta ?? {};
      setCache(route.meta);
      route.meta.renderMenu = hasAuthority(route.meta.permission);
    }
    if (route.children) {
      doMenuFilter(route.children, required);
    }
  });
}

/**
 * 重置过滤
 * @param routes
 */
function resetMenuFilter(routes: Readonly<RouteRecordRaw[]>) {
  const resetCache = (meta: RouteMeta) => {
    if (meta._cache) {
      meta.renderMenu = meta._cache?.renderMenu;
    }
    delete meta._cache;
  };
  routes.forEach((route) => {
    if (route.meta) {
      resetCache(route.meta);
    }
    if (route.children) {
      resetMenuFilter(route.children);
    }
  });
}

// 菜单数据转为路由数据
const toRoutes = (list: MenuProps[]): RouteOption[] => {
  return list.map((item) => ({
    name: item.name,
    path: item.path,
    component: item.component,
    children: item.children && toRoutes(item.children),
    meta: {
      title: item.title,
      permission: item.permission,
      icon: item.icon,
      renderMenu: item.renderMenu,
      cacheable: item.cacheable,
      href: item.link,
      badge: /^(false|true)$/i.test(item.badge + '') ? JSON.parse(item.badge + '') : item.badge,
      target: item.target,
      view: item.view,
    },
  }));
};

export const useMenuStore = defineStore('menu', () => {
  const menuList = ref<MenuProps[]>([]);

  const loading = ref(false);

  const { filterMenu } = storeToRefs(useSettingStore());

  const checkMenuPermission = () => {
    if (filterMenu.value) {
      doMenuFilter(router.options.routes);
      console.log(router.options.routes);
    } else {
      resetMenuFilter(router.options.routes);
    }
  };

  checkMenuPermission();

  watch(filterMenu, checkMenuPermission);

  const presetList = [
    // {
    //   id: 1,
    //   name: 'workplace',
    //   title: '变更记录',
    //   icon: 'DashboardOutlined',
    //   badge: '',
    //   target: '_self',
    //   path: '/workplace',
    //   component: '@/pages/workplace',
    //   renderMenu: true,
    //   parent: null,
    //   permission: null,
    //   cacheable: true,
    // },
    // {
    //   id: 2,
    //   name: 'set',
    //   title: '我的配置',
    //   icon: 'SettingOutlined',
    //   badge: '',
    //   target: '_self',
    //   path: '/set',
    //   component: '@/pages/set',
    //   renderMenu: true,
    //   parent: null,
    //   permission: null,
    //   cacheable: true,
    // },
    // {
    //   id: 3,
    //   name: 'logs',
    //   title: '系统日志',
    //   icon: 'UnorderedListOutlined',
    //   badge: '',
    //   target: '_self',
    //   path: '/logs',
    //   component: '@/pages/logs',
    //   renderMenu: true,
    //   parent: null,
    //   permission: null,
    //   // cacheable: true,
    // }
  ];


  function getlocalMenus() {
    const menuStr = localStorage.getItem('stepin-menu');
    // debugger;
    let menus = [];
    if (!menuStr) {
      menus = presetList;
      localStorage.setItem('stepin-menu', JSON.stringify(menus));
    } else {
      menus = JSON.parse(menuStr);
    }
    // menus = presetList;
    // console.log('menus', menus)
    return menus;
  }




  function getMenuList() {
    loading.value = true;
    let localMenus = getlocalMenus();
    const menuMap = localMenus.reduce((p, c) => {
      p[c.name] = c;
      return p;
    }, {});
    localMenus.forEach((menu) => {
      menu.renderMenu = !!menu.renderMenu;
      if (menu.parent) {
        const parent = menuMap[menu.parent];
        parent.children = parent.children ?? [];
        parent.children.push(menu);
      }
    });
    const res = {
      message: 'success',
      code: 0,
      data: localMenus.filter((menu) => !menu.parent),
    };
    const { data } = res;
    menuList.value = data;
    // console.log(data)
    addRoutes(toRoutes(data));
    checkMenuPermission();

    // loading.value = false;
    setTimeout(() => {
      loading.value = false;
    }, 100);

    return data;

    // return http
    //   .request<MenuProps[], Response<MenuProps[]>>('/menu', 'GET')
    //   .then((res) => {
    //     const { data } = res;
    //     menuList.value = data;
    //     addRoutes(toRoutes(data));
    //     checkMenuPermission();
    //     return data;
    //   })
    //   .finally(() => (loading.value = false));
  }

  async function addMenu(menu: MenuProps) {
    return http
      .request<any, Response<any>>('/menu', 'POST_JSON', menu)
      .then((res) => {
        return res.data;
      })
      .finally(getMenuList);
  }

  async function updateMenu(menu: MenuProps) {
    return http
      .request<any, Response<any>>('/menu', 'PUT_JSON', menu)
      .then((res) => {
        return res.data;
      })
      .finally(getMenuList);
  }

  async function removeMenu(id: number) {
    return http
      .request<any, Response<any>>('/menu', 'DELETE', { id })
      .then(async (res) => {
        if (res.code === 0) {
          removeRoute(res.data.name);
        }
      })
      .finally(getMenuList);
  }

  return {
    loading,
    menuList,
    getMenuList,
    addMenu,
    updateMenu,
    removeMenu,
  };
});
