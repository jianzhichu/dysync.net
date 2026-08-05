<template>
  <ThemeProvider :color="{
      middle: { 'bg-base': 'transparent' },
      primary: { DEFAULT: '#1677ff', hover: '#4096ff' }
    }">
    <section class="login-card">
      <div class="brand-block">
        <img src="/logo.png" alt="抖小云 Logo" class="brand-logo" />
        <h1 class="brand-title">抖小云</h1>
        <p class="brand-subtitle">轻松同步你喜欢的抖音视频</p>
      </div>

      <a-form :model="form" :wrapperCol="{ span: 24 }" @finish="login" class="login-form">
        <a-form-item :required="true" name="username" :validate-status="usernameStatus" class="form-item">
          <a-input v-model:value="form.username" autocomplete="new-username" placeholder="用户名" class="login-input">
            <template #prefix>
              <svg class="input-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
                <circle cx="12" cy="7" r="4"></circle>
              </svg>
            </template>
          </a-input>
        </a-form-item>

        <a-form-item :required="true" name="password" :validate-status="passwordStatus" class="form-item">
          <a-input v-model:value="form.password" autocomplete="new-password" placeholder="密码" class="login-input" :type="showPassword ? 'text' : 'password'">
            <template #prefix>
              <svg class="input-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
                <rect x="3" y="11" width="18" height="10" rx="2"></rect>
                <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
              </svg>
            </template>
            <template #suffix>
              <button class="password-toggle" type="button" @click="showPassword = !showPassword">
                <EyeOutlined v-if="showPassword" aria-label="隐藏密码" />
                <EyeInvisibleOutlined v-else aria-label="显示密码" />
              </button>
            </template>
          </a-input>
        </a-form-item>

        <div class="form-options">
          <a-checkbox v-model:checked="rememberPassword" class="remember-check">记住密码</a-checkbox>
          <button class="forgot-link" type="button" @click="handleForgotPassword">忘记密码?</button>
        </div>

        <a-button htmlType="submit" type="primary" :loading="loading" class="login-button">
          <span v-if="!loading">登录</span>
          <span v-else>登录中...</span>
        </a-button>
      </a-form>
    </section>
  </ThemeProvider>

  <a-modal v-model:visible="forgotModalVisible" width="min(500px, 92vw)" centered :mask-closable="false" :footer="null" wrapClassName="forgot-modal-wrap">
    <template #title>
      <div class="reset-modal-title">
        <span class="reset-modal-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8">
            <rect x="3" y="10" width="18" height="11" rx="2"></rect>
            <path d="M7 10V7a5 5 0 0 1 9.6-2"></path>
            <path d="M16 2v5h5"></path>
          </svg>
        </span>
        <span class="reset-modal-heading">
          <strong>找回登录密码</strong>
          <small>按下面步骤在 NAS 上重置</small>
        </span>
      </div>
    </template>

    <div class="reset-guide">
      <div class="reset-tip">
        这是本地部署工具，密码需要在应用数据目录中重置。
      </div>

      <div class="reset-step">
        <span class="step-number">1</span>
        <div>在 <code>db</code> 目录下新建文本文件，命名为 <code>pwd.txt</code></div>
      </div>
      <div class="reset-step">
        <span class="step-number">2</span>
        <div>写入新的密码；留空则重置为默认密码 <code>douyin2026</code></div>
      </div>
      <div class="reset-step">
        <span class="step-number">3</span>
        <div>重启 Docker 服务或飞牛上的抖小云应用，再使用新密码登录</div>
      </div>

      <div class="reset-actions">
        <a-button type="primary" class="reset-close-button" @click="forgotModalVisible = false">
          我知道了
        </a-button>
      </div>
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
import { reactive, ref, onMounted, computed } from 'vue';
import { useAccountStore } from '@/store';
import { ThemeProvider } from 'stepin';
import { EyeOutlined, EyeInvisibleOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';

// 控制密码显示/隐藏的状态
const showPassword = ref(false);
// 记住密码状态
const rememberPassword = ref(false);
// 加载状态
const loading = ref(false);
// 忘记密码弹窗显示状态
const forgotModalVisible = ref(false);

// 表单状态（用于输入框验证反馈）
const form = reactive({
  username: '',
  password: '',
});
const usernameStatus = computed(() => (form.username ? 'success' : ''));
const passwordStatus = computed(() => (form.password ? 'success' : ''));

export interface LoginFormProps {
  username: string;
  password: string;
}

const emit = defineEmits<{
  (e: 'success', fields: LoginFormProps): void;
  (e: 'failure', reason: string, fields: LoginFormProps): void;
  (e: 'register'): void;
  (e: 'forgot-password'): void;
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

// 登录处理
async function login(params: LoginFormProps) {
  // 简单表单验证
  if (!params.username) {
    message.warning('请输入用户名');
    return;
  }
  if (!params.password) {
    message.warning('请输入密码');
    return;
  }

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

  try {
    const res = await accountStore.login(params.username, params.password);
    emit('success', params);
    console.log(res);
    message.success('登录成功！');
  } catch (e: any) {
    emit('failure', e.message || '登录失败', params);
    message.error(e?.message || e?.data?.message || '登录失败，请重试');
  } finally {
    loading.value = false;
  }
}

// 忘记密码处理 - 打开弹窗
function handleForgotPassword() {
  forgotModalVisible.value = true;
  emit('forgot-password');
}

// 注册处理
function handleRegister() {
  emit('register');
}
</script>

<style scoped lang="less">
.login-card {
  box-sizing: border-box;
  width: min(430px, calc(100vw - 32px));
  padding: 44px 46px 42px;
  border: 1px solid rgba(148, 163, 184, 0.3);
  border-radius: 22px;
  background: linear-gradient(180deg, rgba(17, 29, 48, 0.84), rgba(10, 20, 36, 0.88));
  box-shadow: 0 30px 80px rgba(0, 0, 0, 0.42), inset 0 1px 0 rgba(255, 255, 255, 0.04);
  backdrop-filter: blur(18px);
  -webkit-backdrop-filter: blur(18px);
}

.brand-block {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 30px;
  text-align: center;
}

.brand-logo {
  display: block;
  width: 100px;
  height: 100px;
  margin-bottom: 16px;
  object-fit: contain;
  filter: drop-shadow(0 8px 18px rgba(69, 150, 255, 0.22));
}

.brand-title {
  margin: 0;
  color: #f8fafc;
  font-size: 32px;
  font-weight: 700;
  line-height: 1.25;
  letter-spacing: 0.08em;
}

.brand-subtitle {
  margin: 10px 0 0;
  color: rgba(203, 213, 225, 0.68);
  font-size: 14px;
  line-height: 1.6;
}

.login-form {
  width: 100%;
}

.form-item {
  margin-bottom: 16px;
}

.login-input {
  height: 52px;
  padding: 0 14px !important;
  border: 1px solid rgba(148, 163, 184, 0.28) !important;
  border-radius: 10px !important;
  color: #f8fafc !important;
  font-size: 15px;
  background: rgba(14, 26, 44, 0.72) !important;
  box-shadow: none !important;
  transition: border-color 0.2s ease, background-color 0.2s ease, box-shadow 0.2s ease;
}

.login-input:hover {
  border-color: rgba(148, 163, 184, 0.46) !important;
}

.login-input:focus,
.login-input.ant-input-affix-wrapper-focused {
  border-color: rgba(47, 140, 255, 0.88) !important;
  background: rgba(17, 31, 51, 0.9) !important;
  box-shadow: 0 0 0 3px rgba(47, 140, 255, 0.14) !important;
}

.input-icon {
  width: 20px;
  height: 20px;
  margin-right: 9px;
  color: rgba(203, 213, 225, 0.74);
}

.password-toggle {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  margin: 0;
  padding: 0;
  border: 0;
  color: rgba(203, 213, 225, 0.68);
  font-size: 17px;
  background: transparent;
  cursor: pointer;
  transition: color 0.2s ease;
}

.password-toggle:hover {
  color: #57adff;
}

.form-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 2px 0 22px;
}

.remember-check {
  color: rgba(207, 226, 255, 0.84);
  font-size: 14px;
}

.forgot-link {
  padding: 0;
  border: 0;
  color: #57adff;
  font-size: 14px;
  background: transparent;
  cursor: pointer;
  transition: color 0.2s ease;
}

.forgot-link:hover {
  color: #8ac7ff;
}

.login-button {
  width: 100%;
  height: 52px;
  border: 0 !important;
  border-radius: 10px !important;
  color: #ffffff !important;
  font-size: 16px;
  font-weight: 700;
  letter-spacing: 0.18em;
  background: linear-gradient(135deg, #22c7ff 0%, #1677ff 52%, #3155e7 100%) !important;
  box-shadow: 0 12px 28px rgba(22, 119, 255, 0.28);
  transition: transform 0.18s ease, box-shadow 0.18s ease, filter 0.18s ease;
}

.login-button:hover,
.login-button:focus {
  color: #ffffff !important;
  filter: brightness(1.06);
  transform: translateY(-1px);
  box-shadow: 0 15px 34px rgba(22, 119, 255, 0.34);
}

.login-button:active {
  transform: translateY(0);
}

:deep(.ant-input) {
  color: #f8fafc !important;
  background: transparent !important;
}

:deep(.ant-input::placeholder) {
  color: rgba(148, 163, 184, 0.72) !important;
}

:deep(.ant-form-item-explain-error) {
  color: #fca5a5;
  font-size: 12px;
}

:deep(.ant-checkbox-inner) {
  border-color: rgba(148, 163, 184, 0.54);
  background: rgba(14, 26, 44, 0.9);
}

:deep(.ant-checkbox-checked .ant-checkbox-inner) {
  border-color: #2f8cff !important;
  background: #2f8cff !important;
}

:deep(.ant-checkbox-checked::after) {
  border-color: #2f8cff !important;
}

:deep(.ant-checkbox-wrapper:hover .ant-checkbox-inner),
:deep(.ant-checkbox:hover .ant-checkbox-inner) {
  border-color: #2f8cff !important;
}

.reset-modal-title {
  display: flex;
  align-items: center;
  gap: 12px;
}

.reset-modal-icon {
  flex: 0 0 auto;
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  border: 1px solid rgba(88, 174, 255, 0.28);
  border-radius: 11px;
  color: #66b8ff;
  background: linear-gradient(145deg, rgba(47, 140, 255, 0.18), rgba(22, 119, 255, 0.07));
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.05);
}

.reset-modal-icon svg {
  width: 21px;
  height: 21px;
}

.reset-modal-heading {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.reset-modal-heading strong {
  color: #f8fafc;
  font-size: 17px;
  font-weight: 650;
  line-height: 1.35;
}

.reset-modal-heading small {
  color: rgba(148, 163, 184, 0.76);
  font-size: 12px;
  font-weight: 400;
}

.reset-guide {
  padding: 2px 0 0;
}

.reset-tip {
  margin-bottom: 20px;
  padding: 11px 13px;
  border: 1px solid rgba(56, 143, 255, 0.2);
  border-radius: 10px;
  color: rgba(203, 225, 255, 0.8);
  font-size: 13px;
  line-height: 1.65;
  background: rgba(22, 119, 255, 0.08);
}

.reset-step {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  margin-bottom: 17px;
  color: rgba(226, 232, 240, 0.84);
  font-size: 14px;
  line-height: 1.75;
}

.step-number {
  flex: 0 0 auto;
  display: grid;
  place-items: center;
  width: 25px;
  height: 25px;
  margin-top: 1px;
  border: 1px solid rgba(107, 188, 255, 0.3);
  border-radius: 8px;
  color: #dff1ff;
  font-size: 12px;
  font-weight: 700;
  background: linear-gradient(135deg, rgba(51, 169, 255, 0.92), rgba(22, 119, 255, 0.92));
  box-shadow: 0 6px 16px rgba(22, 119, 255, 0.18);
}

.reset-step code {
  padding: 2px 6px;
  border: 1px solid rgba(79, 163, 255, 0.17);
  border-radius: 5px;
  color: #8ac7ff;
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  background: rgba(47, 140, 255, 0.1);
}

.reset-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: 5px;
}

.reset-close-button {
  min-width: 112px;
  height: 40px;
  border: 0 !important;
  border-radius: 9px !important;
  color: #fff !important;
  font-weight: 600;
  background: linear-gradient(135deg, #27b8ff, #1677ff 58%, #3155e7) !important;
  box-shadow: 0 10px 22px rgba(22, 119, 255, 0.24);
}

.reset-close-button:hover,
.reset-close-button:focus {
  color: #fff !important;
  filter: brightness(1.06);
}

:global(.forgot-modal-wrap .ant-modal-content) {
  overflow: hidden;
  border: 1px solid rgba(125, 178, 235, 0.24);
  border-radius: 18px;
  background: linear-gradient(180deg, rgba(17, 29, 48, 0.98), rgba(8, 18, 33, 0.99));
  box-shadow: 0 30px 90px rgba(0, 0, 0, 0.56), inset 0 1px 0 rgba(255, 255, 255, 0.04);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
}

:global(.forgot-modal-wrap .ant-modal-header) {
  margin: 0;
  padding: 22px 24px 17px;
  border-bottom: 1px solid rgba(148, 163, 184, 0.15);
  background: transparent;
}

:global(.forgot-modal-wrap .ant-modal-title) {
  color: #f8fafc;
  background: transparent;
}

:global(.forgot-modal-wrap .ant-modal-body) {
  padding: 22px 24px 24px;
  color: #e2e8f0;
}

:global(.forgot-modal-wrap .ant-modal-close) {
  top: 18px;
  right: 18px;
  width: 34px;
  height: 34px;
  border-radius: 9px;
  color: rgba(203, 213, 225, 0.72);
  transition: color 0.2s ease, background-color 0.2s ease;
}

:global(.forgot-modal-wrap .ant-modal-close:hover) {
  color: #f8fafc;
  background: rgba(148, 163, 184, 0.1);
}

:global(.forgot-modal-wrap .ant-modal-close-x) {
  width: 34px;
  height: 34px;
  line-height: 34px;
}

@media (max-width: 767.98px) {
  .login-card {
    width: min(390px, calc(100vw - 24px));
    padding: 34px 26px 30px;
    border-radius: 18px;
  }

  .brand-block {
    margin-bottom: 24px;
  }

  .brand-logo {
    width: 80px;
    height: 80px;
    margin-bottom: 12px;
  }

  .brand-title {
    font-size: 27px;
  }

  .brand-subtitle {
    margin-top: 7px;
    font-size: 13px;
  }

  .login-input,
  .login-button {
    height: 48px;
  }
}

@media (max-height: 700px) and (min-width: 768px) {
  .login-card {
    padding-top: 26px;
    padding-bottom: 26px;
  }

  .brand-block {
    margin-bottom: 20px;
  }

  .brand-logo {
    width: 70px;
    height: 70px;
    margin-bottom: 10px;
  }

  .brand-title {
    font-size: 26px;
  }

  .brand-subtitle {
    margin-top: 6px;
  }

  .form-item {
    margin-bottom: 12px;
  }

  .login-input,
  .login-button {
    height: 46px;
  }

  .form-options {
    margin-bottom: 16px;
  }
}
</style>
