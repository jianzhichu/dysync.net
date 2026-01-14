import axios, { AxiosInstance, AxiosRequestConfig, Method as _Method, AxiosResponse } from 'axios';
import qs from 'qs';
import Cookie from 'js-cookie';

declare interface _AxiosExtend {
  /**
   * 发起请求
   * @param url 请求地址
   * @param method 请求方法
   * @param params 请求参数
   * @param config 请求配置（新增上传进度回调）
   */
  request<T = any, R = AxiosResponse<T>>(
    url: string,
    method: Method,
    params?: Record<string | number, any> | FormData,
    config?: AxiosRequestConfig & { onUploadProgress?: (progressEvent: ProgressEvent) => void }
  ): Promise<R>;
  /**
   * 设置token
   * @param value token值
   * @param expires 过期时间
   * - 类型为 number 时，表示 expires 毫秒后 token 过期
   * - 类型为 Date 时，表示在 expires 这个时间点 token 过期
   * @param name token 名称，默认为当前 http 实例的 xsrfCookieName 属性值
   */
  setAuthorization(value: string, expires: number | Date, name?: string): void;

  /**
   * 移出token
   * @param name token 名称， 默认为当前 http 实例的 xsrfCookieName 属性值
   */
  removeAuthorization(name?: string): void;
  /**
   * 校验 token 是否有效
   * @param name 需要校验的 token 名称，默认为当前 http 实例的 xsrfCookieName 属性值
   */
  checkAuthorization(name?: string): boolean;
}

export interface AxiosHttp extends Omit<AxiosInstance, 'request'>, _AxiosExtend { }

export type Method = _Method | 'POST_JSON' | 'post_json' | 'PUT_JSON' | 'put_json' | 'POST_FORM' | 'post_form';

/**
 * 转表单格式
 * @param params
 * @returns
 */
export function toFormData(params?: Record<string | number, any>) {
  const formData = new FormData();
  if (!params) {
    return formData;
  }
  Object.entries(params).forEach(([key, value]) => {
    if (Array.isArray(value)) {
      value.forEach((val) => {
        formData.append(key, val);
      });
    } else {
      formData.set(key, value);
    }
  });
  return formData;
}

function toUrlencoded(params?: Record<string | number, any>) {
  const urlencoded = new URLSearchParams();
  for (const key in params) {
    if (params[key] !== undefined) {
      urlencoded.append(key, params[key]);
    }
  }
  return urlencoded;
}

/**
 * 创建 axios http
 * @param config
 * @returns
 */
function createAxiosHttp(config: AxiosRequestConfig): AxiosHttp {
  const _axios = axios.create(config);

  // 移除 401 处理，仅保留基础的响应拦截器（可选，也可直接删除）
  _axios.interceptors.response.use(
    response => response,
    error => Promise.reject(error)
  );

  const http: AxiosHttp = {
    ..._axios,
    request<T = any, R = AxiosResponse<T>>(
      url: string,
      method: Method,
      params?: Record<string | number, any> | FormData,
      config?: AxiosRequestConfig & { onUploadProgress?: (progressEvent: ProgressEvent) => void }
    ): Promise<R> {
      const _method = method.toUpperCase();
      const requestConfig: AxiosRequestConfig = {
        ...config,
        onUploadProgress: config?.onUploadProgress,
      };

      switch (_method) {
        case 'GET':
          return _axios.get(url, {
            params,
            paramsSerializer: (data) => {
              return qs.stringify(data, { indices: false, skipNulls: true });
            },
            ...requestConfig,
          });
        case 'POST':
          return _axios.post(url, toUrlencoded(params as Record<string | number, any>), requestConfig);
        case 'POST_JSON':
          return _axios.post(url, params, {
            ...requestConfig,
            headers: { 'Content-Type': 'application/json', ...requestConfig.headers },
          });
        case 'POST_FORM':
          return _axios.post(url, params, {
            ...requestConfig,
            headers: { ...requestConfig.headers },
          });
        case 'PUT':
          return _axios.put(url, toFormData(params as Record<string | number, any>), requestConfig);
        case 'PUT_JSON':
          return _axios.put(url, params, {
            ...requestConfig,
            headers: { 'Content-Type': 'application/json', ...requestConfig.headers },
          });
        case 'DELETE':
          return _axios.delete(url, { data: toFormData(params as Record<string | number, any>), ...requestConfig });
        case 'HEAD':
          return _axios.head(url, { params, ...requestConfig });
        case 'OPTIONS':
          return _axios.options(url, { params, ...requestConfig });
        case 'PATCH':
          return _axios.patch(url, { params, ...requestConfig });
        case 'PURGE':
        case 'LINK':
        case 'UNLINK':
          const m = _method as _Method;
          return _axios.request({ url, method: m, params, ..._axios.defaults });
        default:
          return _axios.request({ url, method: 'GET', params, ..._axios.defaults });
      }
    },
    setAuthorization(token: string, expires: number | Date, name?: string): void {
      Cookie.set(name ?? _axios.defaults.xsrfCookieName!, token, { expires });
    },
    removeAuthorization(name?: string): void {
      Cookie.remove(name ?? _axios.defaults.xsrfCookieName!);
    },
    checkAuthorization(name?: string | undefined): boolean {
      return Boolean(Cookie.get(name ?? _axios.defaults.xsrfCookieName!));
    },
  };

  return http;
}

export default createAxiosHttp;