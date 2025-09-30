import { defineStore, storeToRefs } from 'pinia';
import http from './http';
import { ref, watch } from 'vue';
import { Response } from '@/types';
// import { RouteOption } from '@/router/interface';
// import { addRoutes, removeRoute } from '@/router/dynamicRoutes';
// import { useSettingStore } from './setting';
// import { RouteRecordRaw, RouteMeta } from 'vue-router';
// import { useAuthStore } from '@/plugins';
// import router from '@/router';

// export interface MenuProps {
//   id?: number;
//   name: string;
//   path: string;
//   title?: string;
//   icon?: string;
//   badge?: number | string;
//   target?: '_self' | '_blank';
//   link?: string;
//   component: string;
//   renderMenu?: boolean;
//   permission?: string;
//   parent?: string;
//   children?: MenuProps[];
//   cacheable?: boolean;
//   view?: string;
// }

export const useApiStore = defineStore('coreapi', () => {
  //检查是否已经初始化过了
  async function apiCheckInitStatus() {

    return { code: 0 }
    // const initStatus = localStorage.getItem('ddns-init');
    // if (initStatus && initStatus === '1') {
    //   return { code: 0 }
    // } else {
    //   return http
    //     .request<any, Response<any>>('/api/init/Check', 'GET')
    //     .then((res) => {
    //       // console.log(res)
    //       if (res.data.code === 0) {
    //         // localStorage.setItem('ddns-init', '1')
    //       }
    //       return res.data;
    //     })
    //     .finally(() => {

    //     });
    // }
  }

  //初始化系统配置
  async function apiInit(request: object) {
    console.log(request)
    return http
      .request<any, Response<any>>('/api/init/Init', 'post_json', request)
      .then((res) => {
        // console.log(res)

        return res.data;
      })
      .finally(() => {

      });
  }
  //获取配置
  async function apiGetConfig() {
    return http
      .request<any, Response<any>>('/api/config/GetConfig', 'GET')
      .then((res) => {
        console.log(res)
        return res.data;
      })
      .finally(() => {

      });
  }
  //修改配置
  async function apiUpdateConfig(request: object) {
    return http
      .request<any, Response<any>>('/api/config/UpdateConfig', 'post_json', request)
      .then((res) => {
        console.log(res)
        return res.data;
      })
      .finally(() => {

      });
  }
  //后台日志
  async function apiGetLogs(param: string) {
    return http.request<any, Response<any>>('/api/logs/GetLog?' + param, 'get').then(r => {
      // console.log(r)
      return r.data;
    }).finally(() => {

    });
  }
  //用户信息-头像
  async function apiUserInfo() {
    return http.request<any, Response<any>>('/api/auth/GetUserAvatar', 'get').then(r => {
      return r.data;
    }).finally(() => {

    });
  }
  //密码修改
  async function apiChangePwd(param: object) {
    return http.request<any, Response<any>>('/api/auth/UpdatePwd', 'post_json', param).then(r => {
      return r.data;
    }).finally(() => {

    });
  }
  //StartJobNow
  async function StartJobNow() {
    return http.request<any, Response<any>>('/api/config/ExecuteJobNow', 'get').then(r => {
      return r.data;
    }).finally(() => {

    });
  }
  async function VideoStatics() {
    return http.request<any, Response<any>>('/api/video/statics', 'get').then(r => {
      return r.data;
    }).finally(() => {

    });
  }

  //视频查询
  async function VideoPageList(param: object) {
    return http.request<any, Response<any>>('/api/video/paged', 'post_json', param).then(r => {
      return r.data;
    }).finally(() => {

    });
  }
  //cookies
  async function CookiePageList(param: object) {
    return http.request<any, Response<any>>('/api/config/paged', 'post_json', param).then(r => {
      return r.data;
    }).finally(() => {

    });
  }

  async function UpdateConfig(param: object) {
    return http.request<any, Response<any>>('/api/config/update', 'post_json', param).then(r => {
      return r.data;
    }).finally(() => {

    });
  }

  async function deleteCookie(id: string) {
    return http.request<any, Response<any>>('/api/config/delete?id=' + id, 'get').then(r => {
      return r.data;
    }).finally(() => {

    });
  }

  return {
    deleteCookie,
    UpdateConfig,
    apiCheckInitStatus,
    apiInit,
    apiGetConfig,
    apiUpdateConfig,
    apiGetLogs,
    apiUserInfo,
    apiChangePwd,
    StartJobNow,
    VideoStatics,
    VideoPageList,
    CookiePageList
  };
});
