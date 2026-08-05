import axios, { AxiosRequestConfig, AxiosResponse } from 'axios';
import Cookie from 'js-cookie';
import createHttp from '@/utils/axiosHttp';
import { isResponse } from '@/types';
import NProgress from 'nprogress';
import { message } from 'ant-design-vue';
import router from '@/router';

const AUTH_COOKIE_NAME = 'Authorization';
const NETWORK_ERROR_MESSAGE_INTERVAL = 3000;

const http = createHttp({
  timeout: 60000,
  baseURL: '/',
  withCredentials: true,
  xsrfCookieName: AUTH_COOKIE_NAME,
  xsrfHeaderName: AUTH_COOKIE_NAME,
});

let isRedirecting = false;
let lastNetworkErrorMessageAt = 0;

function finishProgress() {
  if (NProgress.isStarted()) {
    NProgress.done();
  }
}

function showNetworkError(error: any) {
  const now = Date.now();
  if (now - lastNetworkErrorMessageAt < NETWORK_ERROR_MESSAGE_INTERVAL) {
    return;
  }

  lastNetworkErrorMessageAt = now;
  const isTimeout = error?.code === 'ECONNABORTED';
  message.error(isTimeout
    ? '接口请求超时，请稍后重试'
    : '无法连接服务器，请检查后台服务或网络状态');
}

function getAuthErrorMessage(authError?: string): string {
  switch (authError) {
    case 'TOKEN_EXPIRED':
      return '登录已超过有效期，请重新登录';
    case 'TOKEN_INVALID':
      return '登录凭证已失效，请重新登录';
    case 'AUTH_REQUIRED':
      return '当前请求缺少登录凭证，请重新登录';
    default:
      return '登录状态无效，请重新登录';
  }
}

http.interceptors.request.use(
  (request: AxiosRequestConfig) => {
    if (!NProgress.isStarted()) {
      NProgress.start();
    }

    // 不再依赖 Axios 的 XSRF 自动转换，显式发送 JWT 请求头。
    const token = Cookie.get(AUTH_COOKIE_NAME);
    if (token) {
      request.headers = request.headers || {};
      if (!(request.headers as Record<string, unknown>).Authorization) {
        (request.headers as Record<string, unknown>).Authorization = token;
      }
    }

    return request;
  },
  (error) => {
    finishProgress();
    return Promise.reject(error);
  }
);

http.interceptors.response.use(
  (response: AxiosResponse) => {
    finishProgress();

    const data = response.data;
    if (isResponse(data)) {
      return data.code === 0
        ? data as any
        : Promise.reject(data);
    }

    // 下载、文本等非标准响应保持原始 data，不再误判成异常。
    return data as any;
  },
  async (error: any) => {
    finishProgress();

    // 页面刷新或主动取消请求时，不提示网络错误，更不能清除登录状态。
    if (axios.isCancel(error) || error?.code === 'ERR_CANCELED') {
      return Promise.reject(error);
    }

    // Network Error 没有 HTTP response，只提示网络状态，不做登出处理。
    if (!error?.response) {
      showNetworkError(error);
      return Promise.reject(error);
    }

    const status = error.response.status;
    const requestUrl = String(error.config?.url || '');
    const isLoginRequest = requestUrl.toLowerCase().includes('/api/auth/login') ||
      requestUrl.toLowerCase().includes('api/auth/login');

    if (status === 401 && !isLoginRequest) {
      if (!isRedirecting) {
        isRedirecting = true;

        // 延迟加载账号 Store，避免 account.ts 与 http.ts 的模块循环依赖。
        const { useAccountStore } = await import('@/store/account');
        const accountStore = useAccountStore();
        accountStore.setLogged(false);
        http.removeAuthorization();

        const authError = String(
          error.response.headers?.['x-auth-error'] ||
          error.response.headers?.['X-Auth-Error'] ||
          ''
        );

        message.warning(getAuthErrorMessage(authError));

        const redirectPath = router.currentRoute.value.fullPath;
        try {
          if (redirectPath !== '/login') {
            await router.replace({
              path: '/login',
              query: { redirect: redirectPath },
            });
          }
        } catch (navigationError) {
          console.error('跳转登录页失败：', navigationError);
        } finally {
          isRedirecting = false;
        }
      }

      return Promise.reject(error);
    }

    return Promise.reject({
      message: error.response.statusText || error.message || '请求失败',
      code: status,
      data: error.response.data,
      originalError: error,
    });
  }
);

export default http;
