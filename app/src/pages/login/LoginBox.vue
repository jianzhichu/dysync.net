<template>
  <ThemeProvider :color="{ 
    middle: { 'bg-base': '#f8fafc' }, // 浅灰底色
    primary: { DEFAULT: '#60a5fa' } // 柔和蓝色作为主色调
  }">
    <!-- 清新风格登录框 -->
    <div class="login-box rounded-xl bg-white shadow-md p-8 max-w-2xl mx-auto my-10 border border-gray-100 transition-all duration-300 hover:shadow-lg">
      <a-form :model="form" :wrapperCol="{ span: 24 }" @finish="login" class="login-form w-full p-lg text-gray-700">
        <div class="third-platform">
          <div class="third-title mb-6 text-xl text-center font-semibold text-gray-800">抖音同步小帮手</div>
        </div>
        <a-divider class="my-6 bg-gray-200"></a-divider>

        <!-- 清新风格输入框 -->
        <a-form-item :required="true" name="username" class="mb-5">
          <a-input v-model:value="form.username" autocomplete="new-username" placeholder="请输入用户名" class="login-input h-[45px] rounded-md bg-gray-50 border-gray-200 text-gray-800 placeholder:text-gray-400 focus:border-primary text-base transition-all focus:ring-1 focus:ring-primary" />
        </a-form-item>

        <a-form-item :required="true" name="password" class="mb-2">
          <a-input v-model:value="form.password" autocomplete="new-password" placeholder="请输入密码" class="login-input h-[45px] rounded-md bg-gray-50 border-gray-200 text-gray-800 placeholder:text-gray-400 focus:border-primary text-base transition-all focus:ring-1 focus:ring-primary" :type="showPassword ? 'text' : 'password'">
            <!-- 密码显示/隐藏图标 -->
            <template #suffix>
              <EyeOutlined v-if="showPassword" @click="showPassword = !showPassword" class="text-gray-400 hover:text-primary cursor-pointer transition-colors" aria-label="隐藏密码" />
              <EyeInvisibleOutlined v-else @click="showPassword = !showPassword" class="text-gray-400 hover:text-primary cursor-pointer transition-colors" aria-label="显示密码" />
            </template>
          </a-input>
        </a-form-item>

        <!-- 记住密码选项 -->
        <a-form-item class="mb-6 pl-2">
          <a-checkbox v-model:checked="rememberPassword" class="text-gray-600 hover:text-primary transition-colors">
            记住密码
          </a-checkbox>
        </a-form-item>

        <a-button htmlType="submit" class="h-[48px] w-full rounded-md transition-colors hover:bg-primary/90 bg-primary border-primary text-white text-base shadow-sm hover:shadow" type="primary" :loading="loading">
          登录
        </a-button>
      </a-form>
    </div>
  </ThemeProvider>
</template>

<script lang="ts" setup>
import { reactive, ref, onMounted } from 'vue';
import { useAccountStore } from '@/store';
import { ThemeProvider } from 'stepin';
import { EyeOutlined, EyeInvisibleOutlined } from '@ant-design/icons-vue';

// 控制密码显示/隐藏的状态
const showPassword = ref(false);
// 记住密码状态
const rememberPassword = ref(false);

export interface LoginFormProps {
  username: string;
  password: string;
}
const loading = ref(false);

const form = reactive<LoginFormProps>({
  username: '',
  password: '',
});

const emit = defineEmits<{
  (e: 'success', fields: LoginFormProps): void;
  (e: 'failure', reason: string, fields: LoginFormProps): void;
}>();

const accountStore = useAccountStore();

// 页面加载时读取本地存储的记住密码信息
onMounted(() => {
  const savedUser = localStorage.getItem('rememberedUser');
  if (savedUser) {
    try {
      const { username, password } = JSON.parse(savedUser);
      form.username = username;
      form.password = password;
      rememberPassword.value = true;
    } catch (e) {
      console.error('读取保存的用户信息失败', e);
      localStorage.removeItem('rememberedUser');
    }
  }
});

function login(params: LoginFormProps) {
  loading.value = true;

  // 根据记住密码状态保存/清除用户信息
  if (rememberPassword.value) {
    localStorage.setItem(
      'rememberedUser',
      JSON.stringify({
        username: params.username,
        password: params.password,
      })
    );
  } else {
    localStorage.removeItem('rememberedUser');
  }

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