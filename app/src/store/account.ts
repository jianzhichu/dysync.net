import { defineStore } from 'pinia';
import http from './http';
import { Response } from '@/types';
import { useMenuStore } from './menu';
import { useAuthStore } from '@/plugins';

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
  code: number,
  erro: string,
};
export const useAccountStore = defineStore('account', {
  state() {
    return {
      account: {} as Account,
      permissions: [] as string[],
      role: '',
      logged: true,
      logged2: false
    };
  },
  actions: {
    async login(username: string, password: string) {
      return http
        .request<TokenResult, Response<TokenResult>>('api/auth/login', 'post_json', { username, password })
        .then(async (response) => {
          if (response.code === 200 && response.data.code === 0) {
            this.logged = true;
            this.logged2 = true
            // console.log(response)
            http.setAuthorization(`Bearer ${response.data.token}`, response.data.expires);
            // await useMenuStore().getMenuList();
            return response.data;
          } else {
            this.logged2 = false
            return Promise.reject(response);
          }
        });
    },
    async logout() {
      return new Promise<boolean>((resolve) => {
        localStorage.removeItem('stepin-menu');
        http.removeAuthorization();
        // this.logged = false;
        resolve(true);
      });
    },
    async profile() {
      return http.request<Account, Response<Profile>>('/account', 'get').then((response) => {
        if (response.code === 0) {
          const { setAuthorities } = useAuthStore();
          const { account, permissions, role } = response.data;
          this.account = account;
          this.permissions = permissions;
          this.role = role;
          setAuthorities(permissions);
          return response.data;
        } else {
          return Promise.reject(response);
        }
      });
    },
    setLogged(logged: boolean) {
      this.logged = logged;
    },
  },
});
