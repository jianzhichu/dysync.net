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


  async function apiGetConfig() {
    return http.request<any, Response<any>>('/api/config/GetConfig', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  // //获取配置
  // async function apiGetConfig() {
  //   return http
  //     .request<any, Response<any>>('/api/config/GetConfig', 'GET')
  //     .then((res) => {
  //       return res;
  //     })
  //     .finally(() => {

  //     });
  // }
  //修改配置
  async function apiUpdateConfig(request: object) {
    return http
      .request<any, Response<any>>('/api/config/UpdateConfig', 'post_json', request)
      .then((res) => {
        console.log(res)
        return res;
      })
      .finally(() => {

      });
  }
  //后台日志
  async function apiGetLogs(param: string) {
    return http.request<any, Response<any>>('/api/logs/GetLog/' + param, 'get').then(r => {
      // console.log(r)
      return r.data;
    }).finally(() => {

    });
  }
  //用户信息-头像
  async function apiUserInfo() {
    return http.request<any, Response<any>>('/api/auth/GetUserAvatar', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //密码修改
  async function apiChangePwd(param: object) {
    return http.request<any, Response<any>>('/api/auth/UpdatePwd', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //StartJobNow
  async function StartJobNow() {
    return http.request<any, Response<any>>('/api/config/ExecuteJobNow', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  async function VideoStatics() {
    return http.request<any, Response<any>>('/api/video/statics', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  //视频查询
  async function VideoPageList(param: object) {
    return http.request<any, Response<any>>('/api/video/paged', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //cookies
  async function CookiePageList(param: object) {
    return http.request<any, Response<any>>('/api/config/paged', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  async function CookieList() {
    return http.request<any, Response<any>>('/api/config/list', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }


  async function UpdateConfig(param: object) {
    return http.request<any, Response<any>>('/api/config/update', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  async function DeskInitAsync(param: object) {
    return http.request<any, Response<any>>('/api/config/deskinit', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }

  async function AppisInit() {
    return http.request<any, Response<any>>('/api/config/isInit', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }


  async function deleteCookie(id: string) {
    return http.request<any, Response<any>>('/api/config/delete?id=' + id, 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //follows
  async function FollowList(param: object) {
    return http.request<any, Response<any>>('/api/follow/paged', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //同步关注列表
  async function SyncFollow() {
    return http.request<any, Response<any>>('/api/follow/sync', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //更新同步关注者状态
  async function OpenOrCloseSync(param: object) {
    return http.request<any, Response<any>>('/api/follow/openOrCloseSync', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //更新同步关注者状态
  async function OpenOrCloseFullSync(param: object) {
    return http.request<any, Response<any>>('/api/follow/openOrCloseFullSync', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //重新下载
  async function ReDownViedos(param: object) {
    return http.request<any, Response<any>>('/api/video/redown', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //批量删除
  async function BathRealDelete(param: object) {
    return http.request<any, Response<any>>('/api/video/vdelete/batch', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //删除
  async function DeleteVideo(param: string) {
    return http.request<any, Response<any>>('/api/video/vdelete/' + param, 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //查询已删除
  async function GetDeleteViedos() {
    return http.request<any, Response<any>>('/api/video/vdelete/get', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //删除博主全部视频
  async function DeleteByAuthor(param: string) {
    return http.request<any, Response<any>>('/api/video/vdelete/byauthor/' + param, 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  //检查版本
  async function getVer() {
    return http.request<any, Response<any>>('/api/config/mytag', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //检查版本
  async function CheckTag() {
    return http.request<any, Response<any>>('/api/config/checktag', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  async function mp3List() {
    return http.request<any, Response<any>>('/api/config/mp3List', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }


  //快速停止或启动cookie配置
  async function SwitchCookieStatus(param: object) {
    return http.request<any, Response<any>>('/api/config/switch', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }

  //添加非关注的博主
  async function AddFollow(param: object) {
    return http.request<any, Response<any>>('/api/follow/add', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //删除非关注的博主
  async function DelFollow(param: object) {
    return http.request<any, Response<any>>('/api/follow/delete', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }

  //导出配置
  async function ExportConf() {
    return http.request<any, Response<any>>('/api/config/exportConf', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  //导入配置
  async function ImportConf(param: object) {
    return http.request<any, Response<any>>('/api/config/importConf', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }

  //移动端获取日志列表
  async function MobileLogs() {
    return http.request<any, Response<any>>('/api/logs/list', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  //移动端获取日志详情
  async function LogDetail(type: string, date: string) {
    return http.request<any, Response<any>>('/api/logs/content?type=' + type + "&Date=" + date, 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  //TOP
  async function TopVideo(param: number) {
    return http.request<any, Response<any>>('/api/Video/top' + param, 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }

  //Renfo
  async function Renfo() {
    return http.request<any, Response<any>>('/api/Video/renfo', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }


  // // 音频文件上传接口
  // async function apiUploadAudio(formData: FormData, options?: { onUploadProgress?: (progressEvent: ProgressEvent) => void }) {
  //   return http
  //     .request<any, Response<any>>(
  //       '/api/config/UploadAudio',  // 请求地址
  //       'post_form',                // 使用新增的 post_form 类型
  //       formData,                   // FormData 参数（文件+其他参数）
  //       {
  //         onUploadProgress: options?.onUploadProgress, // 上传进度回调（原生 ProgressEvent）
  //         timeout: 120000 // 上传文件超时时间设为2分钟（可选）
  //       }
  //     )
  //     .then((res) => {
  //       // console.log('音频上传结果：', res);
  //       // 适配你的响应格式（如果响应是包裹层，取 data）
  //       return res;
  //     })
  //     .catch((err) => {
  //       console.error('音频上传失败：', err);
  //       throw err; // 抛出错误让前端捕获
  //     });
  // }

  return {
    getVer,
    mp3List,
    BathRealDelete,
    DeleteByAuthor,
    Renfo,
    // apiUploadAudio,
    AppisInit,
    DeskInitAsync,
    SwitchCookieStatus,
    TopVideo,
    LogDetail,
    MobileLogs,
    ExportConf,
    ImportConf,
    GetDeleteViedos,
    DelFollow,
    AddFollow,
    CheckTag,
    deleteCookie,
    UpdateConfig,
    apiGetConfig,
    apiUpdateConfig,
    apiGetLogs,
    apiUserInfo,
    apiChangePwd,
    StartJobNow,
    VideoStatics,
    VideoPageList,
    CookiePageList,
    CookieList,
    FollowList,
    SyncFollow,
    OpenOrCloseSync,
    OpenOrCloseFullSync,
    ReDownViedos,
    DeleteVideo
  };
});
