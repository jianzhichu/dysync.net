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

export const VERSION_CACHE_UPDATED_EVENT =
  'dysync:version-cache-updated';

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
  // 后台日志
  async function apiGetLogs(param: string): Promise<string> {
    const safePath = param
      .split('/')
      .map((part) => encodeURIComponent(part))
      .join('/');

    return http
      .request<string, Response<string>>(
        '/api/logs/GetLog/' + safePath,
        'get',
        {
          // 避免旧版本后端、代理服务器或浏览器继续复用304缓存。
          _ts: Date.now(),
        }
      )
      .then((response) => {
        // 兼容上一版后端误把字符串日志放入 message 的情况。
        const content = response.data ??
          (response.message && response.message !== '操作成功'
            ? response.message
            : '');

        return String(content);
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
  //视频统计
  async function VideoStatics() {
    return http.request<any, Response<any>>('/api/video/statics', 'get').then(r => {
      return r;
    }).finally(() => {

    });
  }
  // 视频作者统计分页
  async function VideoAuthorStatics(pageIndex: number, pageSize: number) {
    return http.request<any, Response<any>>(
      `/api/video/statics/authors?pageIndex=${pageIndex}&pageSize=${pageSize}`,
      'get'
    );
  }
  //视频曲线
  async function VideoChart(day: number) {
    return http.request<any, Response<any>>(`/api/video/chart/${day}`, 'get').then(r => {
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

  async function GetDatabaseStatus() {
    return http.request<any, Response<any>>('/api/config/database/status', 'get');
  }

  async function MigrateDatabase(param: object) {
    return http.request<any, Response<any>>('/api/config/database/migrate', 'post_json', param, {
      timeout: 30 * 60 * 1000,
    });
  }

  async function SelectSqliteDatabase() {
    return http.request<any, Response<any>>('/api/config/database/select-sqlite', 'post_json', {});
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

  // 版本信息只允许两种情况访问后台：
  // 1. 登录成功后强制刷新一次；
  // 2. 用户手动点击“获取版本”时强制刷新。
  // 普通组件挂载和 F5 刷新只能读取 localStorage 缓存。
  const GET_VER_CACHE_KEY = 'coreapi:getVer:v2';
  const CHECK_TAG_CACHE_KEY = 'coreapi:checkTag:v2';

  let getVerPending: Promise<Response<any>> | null = null;
  let checkTagPending: Promise<Response<any>> | null = null;

  type VersionCacheType = 'getVer' | 'checkTag';

  type VersionRefreshResult = {
    getVer: Response<any> | null;
    checkTag: Response<any> | null;
    errors: unknown[];
  };

  function createEmptyVersionResponse(
    message = '暂无版本缓存'
  ): Response<any> {
    return {
      code: 0,
      message,
      data: null,
    };
  }

  function readVersionCache(
    cacheKey: string
  ): Response<any> | null {
    try {
      const cacheText = localStorage.getItem(cacheKey);
      if (!cacheText) {
        return null;
      }

      const cache = JSON.parse(cacheText) as {
        value?: Response<any>;
      };

      if (!cache?.value) {
        localStorage.removeItem(cacheKey);
        return null;
      }

      return cache.value;
    } catch (error) {
      try {
        localStorage.removeItem(cacheKey);
      } catch {
        // 浏览器禁用 localStorage 时忽略清理异常。
      }

      console.warn('读取版本缓存失败：', error);
      return null;
    }
  }

  function notifyVersionCacheUpdated(
    type: VersionCacheType
  ) {
    if (typeof window === 'undefined') {
      return;
    }

    window.dispatchEvent(
      new CustomEvent(VERSION_CACHE_UPDATED_EVENT, {
        detail: { type },
      })
    );
  }

  function writeVersionCache(
    cacheKey: string,
    value: Response<any>,
    type: VersionCacheType
  ) {
    try {
      localStorage.setItem(
        cacheKey,
        JSON.stringify({
          updatedAt: Date.now(),
          value,
        })
      );

      notifyVersionCacheUpdated(type);
    } catch (error) {
      console.warn('保存版本缓存失败：', error);
    }
  }

  function getCachedVer(): Response<any> | null {
    return readVersionCache(GET_VER_CACHE_KEY);
  }

  function getCachedCheckTag(): Response<any> | null {
    return readVersionCache(CHECK_TAG_CACHE_KEY);
  }

  function clearVersionCaches() {
    try {
      localStorage.removeItem(GET_VER_CACHE_KEY);
      localStorage.removeItem(CHECK_TAG_CACHE_KEY);

      // 清理旧版本曾经使用过的缓存键。
      localStorage.removeItem('coreapi:getVer');
      localStorage.removeItem('dysync_version_cache');
    } catch (error) {
      console.warn('清理版本缓存失败：', error);
    }
  }

  // 获取当前部署版本（/api/config/mytag）。
  // forceRefresh=false 时绝不访问后台，只返回缓存。
  async function getVer(
    forceRefresh = false
  ): Promise<Response<any>> {
    if (!forceRefresh) {
      return (
        getCachedVer() ??
        createEmptyVersionResponse('暂无当前版本缓存')
      );
    }

    if (getVerPending) {
      return getVerPending;
    }

    getVerPending = http
      .request<any, Response<any>>(
        '/api/config/mytag',
        'get'
      )
      .then((response) => {
        if (response.code === 0) {
          writeVersionCache(
            GET_VER_CACHE_KEY,
            response,
            'getVer'
          );
        }

        return response;
      })
      .finally(() => {
        getVerPending = null;
      });

    return getVerPending;
  }

  // 获取版本列表（/api/config/checktag）。
  // forceRefresh=false 时绝不访问后台，只返回缓存。
  async function CheckTag(
    forceRefresh = false
  ): Promise<Response<any>> {
    if (!forceRefresh) {
      return (
        getCachedCheckTag() ??
        createEmptyVersionResponse('暂无版本列表缓存')
      );
    }

    if (checkTagPending) {
      return checkTagPending;
    }

    checkTagPending = http
      .request<any, Response<any>>(
        '/api/config/checktag',
        'get'
      )
      .then((response) => {
        if (response.code === 0) {
          writeVersionCache(
            CHECK_TAG_CACHE_KEY,
            response,
            'checkTag'
          );
        }

        return response;
      })
      .finally(() => {
        checkTagPending = null;
      });

    return checkTagPending;
  }

  // 同时刷新两个版本接口。
  // 登录成功和手动“获取版本”都调用这个方法。
  async function refreshVersionInfo(
    forceRefresh = true
  ): Promise<VersionRefreshResult> {
    const [getVerResult, checkTagResult] =
      await Promise.allSettled([
        getVer(forceRefresh),
        CheckTag(forceRefresh),
      ]);

    const errors: unknown[] = [];

    if (getVerResult.status === 'rejected') {
      errors.push(getVerResult.reason);
    }

    if (checkTagResult.status === 'rejected') {
      errors.push(checkTagResult.reason);
    }

    return {
      getVer:
        getVerResult.status === 'fulfilled'
          ? getVerResult.value
          : getCachedVer(),
      checkTag:
        checkTagResult.status === 'fulfilled'
          ? checkTagResult.value
          : getCachedCheckTag(),
      errors,
    };
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

  //合集、自定义收藏夹、短剧列表
  async function CatePageList(param: object) {
    return http.request<any, Response<any>>('/api/cate/paged', 'post_json', param).then(r => {
      return r;
    }).finally(() => {

    });
  }
  //批量修改 合集、自定义收藏夹、短剧
  async function BatchSaveCate(param: object) {
    return http.request<any, Response<any>>('/api/cate/BatchSave', 'post_json', param).then(r => {
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
    VideoChart,
    BatchSaveCate,
    CatePageList,
    getVer,
    getCachedVer,
    getCachedCheckTag,
    refreshVersionInfo,
    clearVersionCaches,
    mp3List,
    BathRealDelete,
    DeleteByAuthor,
    Renfo,
    // apiUploadAudio,
    AppisInit,
    GetDatabaseStatus,
    MigrateDatabase,
    SelectSqliteDatabase,
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
    VideoAuthorStatics,
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
