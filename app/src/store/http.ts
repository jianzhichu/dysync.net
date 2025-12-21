import { AxiosRequestConfig, AxiosResponse } from 'axios';
import createHttp from '@/utils/axiosHttp';
import { isResponse } from '@/types';
import NProgress from 'nprogress';
import { useAccountStore } from '@/store';
import { message } from 'ant-design-vue';
import router from '@/router'; // 关键：导入路由实例（路径要和实际一致）
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

// progress 进度条 -- 开启
http.interceptors.request.use((req: AxiosRequestConfig) => {
  if (!NProgress.isStarted()) {
    NProgress.start();
  }
  return req;
});

// 解析响应结果
http.interceptors.response.use(
  (rep: AxiosResponse<String>) => {
    const { data } = rep;
    if (isResponse(data)) {
      return data.code === 0 ? data : Promise.reject(data);
    }
    return Promise.reject({ message: rep.statusText, code: rep.status, data });
  },
  (error) => {
    if (error.response.status === 401) {
      const accountStore = useAccountStore();
      // 1. 清除登录状态
      accountStore.setLogged(false);
      // 可选：提示用户登录过期
      message.warning('登录状态已过期，请重新登录'); // 如使用Element Plus


      setTimeout(() => {
        const redirectPath = router.currentRoute.value.fullPath;
        router.push({
          path: '/login',
          query: { redirect: redirectPath }
        }).then(() => {
          console.log('跳转登录页成功');
        }).catch((err) => {
          console.error('跳转登录页失败：', err); // 关键！捕获跳转失败的原因
        });
      }, 100);

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

// progress 进度条 -- 关闭
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
    return error;
  }
);

export default http;
