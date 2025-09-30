<template>
  <ThemeProvider :color="{ middle: { 'bg-base': '#1a1a1a' }, primary: { DEFAULT: '#36bffA' } }">
    <!-- 加宽后的登录框，使用更大的宽度设置 -->
    <div class="login-box rounded-lg bg-gray-900 shadow-lg p-8 max-w-2xl mx-auto my-10 border border-gray-800 transition-all duration-300 hover:shadow-xl">
      <a-form :model="form" :wrapperCol="{ span: 24 }" @finish="login" class="login-form w-full p-lg text-gray-200">
        <div class="third-platform">
          <div class="third-title mb-6 text-xl text-center font-semibold text-white">抖音同步工具</div>
        </div>
        <a-divider class="my-6 bg-gray-700"></a-divider>

        <!-- 增加输入框宽度并调整间距 -->
        <a-form-item :required="true" name="username" class="mb-5">
          <a-input v-model:value="form.username" autocomplete="new-username" placeholder="请输入用户名" class="login-input h-[45px] rounded-md bg-gray-800 border-gray-700 text-white placeholder:text-gray-500 focus:border-primary text-lg" />
        </a-form-item>

        <a-form-item :required="true" name="password" class="mb-6">
          <a-input v-model:value="form.password" autocomplete="new-password" placeholder="请输入密码" class="login-input h-[45px] rounded-md bg-gray-800 border-gray-700 text-white placeholder:text-gray-500 focus:border-primary text-lg" type="password" />
        </a-form-item>

        <a-button htmlType="submit" class="h-[48px] w-full rounded-md transition-colors hover:opacity-90 bg-primary border-primary text-lg" type="primary" :loading="loading">
          登录
        </a-button>
      </a-form>
    </div>
  </ThemeProvider>
</template>

<script lang="ts" setup>
import { reactive, ref } from 'vue';
import { useAccountStore } from '@/store';
import { ThemeProvider } from 'stepin';

export interface LoginFormProps {
  username: string;
  password: string;
}
const loading = ref(false);

const form = reactive({
  username: undefined,
  password: undefined,
});

const emit = defineEmits<{
  (e: 'success', fields: LoginFormProps): void;
  (e: 'failure', reason: string, fields: LoginFormProps): void;
}>();

const accountStore = useAccountStore();
function login(params: LoginFormProps) {
  loading.value = true;
  accountStore
    .login(params.username, params.password)
    .then((res) => {
      emit('success', params);
    })
    .catch((e) => {
      emit('failure', e.message, e.data);
    })
    .finally(() => (loading.value = false));
}
</script>
