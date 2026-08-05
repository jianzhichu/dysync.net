import { defineStore } from 'pinia';
import http from './http';
import { Response } from '@/types';
import { useAuthStore } from '@/plugins';
import { useApiStore } from './coreapi';

export interface Profile {
  account: Account;
  permissions: string[];
  role: string;
}

export interface Account {
  username: string;
  avatar: string;
  gender: number;
}

export type TokenResult = {
  token: string;
  expires: number;
  expiresAt?: string;
  userName: string;
};

export const useAccountStore = defineStore('account', {
  state() {
    return {
      account: {} as Account,
      permissions: [] as string[],
      role: '',
      logged: true,
      logged2: false,
    };
  },
  actions: {
    async login(username: string, password: string) {
      const response = await http.request<
        TokenResult,
        Response<TokenResult>
      >(
        '/api/auth/login',
        'post_json',
        { username, password }
      );

      http.setAuthorization(
        `Bearer ${response.data.token}`,
        response.data.expires
      );

      // 登录完成后结束本次登录前启动周期。
      // 将来退出或 Token 失效后，再次进入登录页时可重新展示启动页。
      try {
        sessionStorage.removeItem(
          'dysync-login-startup-shown-v2'
        );
        sessionStorage.removeItem(
          'dysync-login-startup-shown'
        );
      } catch (error) {
        console.warn('无法清理登录启动页状态：', error);
      }

      this.logged = true;
      this.logged2 = true;

      // 每次登录成功后仅调用一次 getver 和 checktag。
      // 不阻塞登录跳转；结果写入 localStorage，后续 F5 只读缓存。
      const apiStore = useApiStore();
      apiStore.clearVersionCaches();

      void apiStore
        .refreshVersionInfo(true)
        .then((result) => {
          if (result.errors.length > 0) {
            console.warn(
              '登录后的版本信息获取未全部成功：',
              result.errors
            );
          }
        })
        .catch((error) => {
          // 版本接口失败不能影响正常登录。
          console.warn('登录后获取版本信息失败：', error);
        });

      return response.data;
    },

    async logout() {
      return new Promise<boolean>((resolve) => {
        localStorage.removeItem('stepin-menu');
        http.removeAuthorization();

        try {
          sessionStorage.removeItem(
            'dysync-login-startup-shown-v2'
          );
          sessionStorage.removeItem(
            'dysync-login-startup-shown'
          );
        } catch (error) {
          console.warn('无法重置登录启动页状态：', error);
        }

        this.logged = false;
        this.logged2 = false;
        resolve(true);
      });
    },

    async profile() {
      return http
        .request<Account, Response<Profile>>(
          '/account',
          'get'
        )
        .then((response) => {
          if (response.code === 0) {
            const { setAuthorities } = useAuthStore();
            const {
              account,
              permissions,
              role,
            } = response.data;

            this.account = account;
            this.permissions = permissions;
            this.role = role;
            setAuthorities(permissions);

            return response.data;
          }

          return Promise.reject(response);
        });
    },

    setLogged(logged: boolean) {
      this.logged = logged;
    },
  },
});
