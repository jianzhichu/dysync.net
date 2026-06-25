// src/store/routeParam.ts
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const useRouteParamStore = defineStore('routeParam', () => {
  // 记录页跳转参数
  const workplaceAuthor = ref('');

  // 设置参数
  const setWorkplaceAuthor = (name: string) => {
    workplaceAuthor.value = name;
  };

  // 清空参数
  const clearWorkplaceAuthor = () => {
    workplaceAuthor.value = '';
  };

  return {
    workplaceAuthor,
    setWorkplaceAuthor,
    clearWorkplaceAuthor,
  };
});