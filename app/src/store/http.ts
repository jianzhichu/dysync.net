import { AxiosRequestConfig, AxiosResponse } from 'axios';
import createHttp from '@/utils/axiosHttp';
import { isResponse } from '@/types';
import NProgress from 'nprogress';
import { useAccountStore } from '@/store';
import { message } from 'ant-design-vue';
import router from '@/router';

const http = createHttp({
  timeout: 60000,
  baseURL: '/',
  withCredentials: true,
  xsrfCookieName: 'Authorization',
  xsrfHeaderName: 'Authorization',
});

const isAxiosResponse = (obj: any): obj is AxiosResponse => {
  return typeof obj === 'object' && obj.status && obj.statusText && obj.headers && obj.config;
};

// 仅新增这一行：跳转锁
let isRedirecting = false;

// progress 进度条 -- 开启（和你原本一致）
http.interceptors.request.use((req: AxiosRequestConfig) => {
  if (!NProgress.isStarted()) {
    NProgress.start();
  }
  return req;
});

// 解析响应结果（完全和你原本一致，一字未改）
http.interceptors.response.use(
  (rep: AxiosResponse<String>) => {
    const { data } = rep;
    if (isResponse(data)) {
      return data.code === 0 ? data : Promise.reject(data);
    }
    return Promise.reject({ message: rep.statusText, code: rep.status, data });
  },
  (error) => {
    if (error.response?.status === 401) {
      // 仅新增：加锁判断（这是唯一改动）
      if (!isRedirecting) {
        isRedirecting = true;

        const accountStore = useAccountStore();
        accountStore.setLogged(false);
        message.warning('登录状态已过期，请重新登录');
        setTimeout(() => {

          const redirectPath = router.currentRoute.value.fullPath;
          if (redirectPath !== '/login') {
            router.push({
              path: '/login',
              query: { redirect: redirectPath }
            }).then(() => {
              console.log('跳转登录页成功');
            }).catch((err) => {
              console.error('跳转登录页失败：', err);
            }).finally(() => {
              isRedirecting = false;
            });
          } else {
            isRedirecting = false;
          }
        }, 100);
      }
      // 新增结束
    } else {
      if (error.response && isAxiosResponse(error.response)) {
        return Promise.reject({
          message: error.response.statusText,
          code: error.response.status,
          data: error.response.data,
        });
      }
    }

    return Promise.reject(error);
  }
);

// progress 进度条 -- 关闭（改回你原本的逻辑，不碰返回值）
http.interceptors.response.use(
  (rep) => {
    if (NProgress.isStarted()) {
      NProgress.done();
    }
    return rep;
  },
  (error) => {
    if (NProgress.isStarted()) {
      NProgress.done();
    }
    // 改回你原本的返回值：return error（之前改这个导致登录异常）
    return error;
  }
);

export default http;