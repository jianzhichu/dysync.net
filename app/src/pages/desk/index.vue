<script lang="ts" setup>
import { reactive, ref, watch, onMounted } from 'vue';
import { FormInstance } from 'ant-design-vue';
import { message } from 'ant-design-vue';
import { useApiStore, useAccountStore } from '@/store';
import { useRouter } from 'vue-router';

import {
  CloudServerOutlined,
  ExclamationCircleOutlined,
  FolderOpenOutlined,
  KeyOutlined,
  SaveOutlined,
} from '@ant-design/icons-vue';
import DatabaseConfigFields from '@/components/setting/DatabaseConfigFields.vue';

type ConfigItem = {
  id?: string;
  userName: string; // 改为非可选，强制初始化
  cookies: string;
  savePath: string;
  favSavePath: string;
  secUserId: string;
  status: number;
  upSavePath: string;
  // imgSavePath: string;
  useSinglePath: boolean; // 非可选
  databaseType: string;
  databaseHost: string;
  databasePort: number;
  databaseUserName: string;
  databasePassword: string;
  databaseName: string;
};

// 强制初始化所有字段为非undefined值
const newConfig = (): ConfigItem => {
  return {
    id: '0',
    userName: '', // 强制空字符串
    cookies: '',
    savePath: '',
    favSavePath: '',
    secUserId: '',
    status: 0,
    upSavePath: '',
    // imgSavePath: '',
    useSinglePath: true,
    databaseType: 'Sqlite',
    databaseHost: '',
    databasePort: 3306,
    databaseUserName: '',
    databasePassword: '',
    databaseName: '',
  };
};

// 改用 ref 而非 reactive（解决深层响应式问题）
const form = ref<ConfigItem>(newConfig());
const formModel = ref<FormInstance>();
const formLoading = ref(false);

// 监听统一路径（改用 ref 的取值方式）
watch(
  [() => form.value.savePath, () => form.value.useSinglePath],
  ([newSavePath, useSinglePath]) => {
    if (useSinglePath && newSavePath) {
      form.value.favSavePath = newSavePath;
      form.value.upSavePath = newSavePath;
      // form.value.imgSavePath = newSavePath;
    }
  },
  { immediate: true }
);

// 校验规则：明确trigger + 不依赖组件自动触发
const formRules = reactive({
  userName: [{ required: true, message: '请输入Cookie名称', trigger: 'change' }],
  savePath: [{ required: true, message: '请输入视频存储路径', trigger: 'change' }],
  secUserId: [{ required: false, message: '请输入我的secUserId', trigger: 'change' }],
});

const router = useRouter();
onMounted(() => {
  const accountStore = useAccountStore(); // 移到内部避免提前引用
  useApiStore()
    .AppisInit()
    .then((res) => {
      if (res.code == 0 && res.data) {
        if (accountStore.logged) {
          router.push('/dashboard');
        } else {
          router.push('/login');
        }
      }
    });
});

// 提交：手动指定校验字段 + 强制取值
const submitConfig = async () => {
  formLoading.value = true;
  try {
    // 手动获取表单实例，防止null
    const formInstance = formModel.value;
    if (!formInstance) {
      message.error('表单实例未初始化');
      formLoading.value = false;
      return;
    }

    // 手动校验（兜底方案）
    const checkResult = manualCheckForm();
    if (!checkResult.pass) {
      message.error(checkResult.msg);
      formLoading.value = false;
      return;
    }
    // 提交时强制解构，确保传递最新值
    const res = await useApiStore().DeskInitAsync({ ...form.value });
    if (res.code === 0) {
      const databaseChanged = form.value.databaseType !== 'Sqlite';
      message.success(databaseChanged ? '配置和数据迁移成功，后台服务正在重启...' : '配置保存成功,即将跳转登录页...');
      setTimeout(() => {
        if (databaseChanged) {
          window.location.href = '/login';
        } else {
          router.push('/login');
        }
      }, databaseChanged ? 4000 : 500);
    } else {
      message.error(`保存失败：${res.message || '未知错误'}`);
    }
  } catch (err: any) {
    console.error('初始化配置提交失败：', err);
    const errorData = err?.response?.data ?? err;
    const errorMessage =
      err?.errorFields?.[0]?.errors?.[0] ||
      errorData?.message ||
      errorData?.msg ||
      errorData?.error ||
      err?.message ||
      '保存失败，请稍后重试';
    message.error(errorMessage, 8);
  } finally {
    formLoading.value = false;
  }
};

// 手动校验函数（兜底方案）
const manualCheckForm = (): { pass: boolean; msg: string } => {
  if (!form.value.userName.trim()) {
    return { pass: false, msg: '请输入Cookie名称' };
  }
  if (!form.value.savePath.trim()) {
    return { pass: false, msg: '请输入视频存储路径' };
  }
  if (form.value.databaseType !== 'Sqlite' &&
      (!form.value.databaseHost.trim() || !form.value.databaseUserName.trim() || !form.value.databasePassword)) {
    return { pass: false, msg: '请完整填写数据库 Host、端口、账号和密码' };
  }
  return { pass: true, msg: '' };
};
</script>

<template>
  <div class="init-config-page">
    <div class="page-header">
      <div class="header-content">
        <div class="brand-row">
          <div class="logo-container">
            <img src="/logo.png" alt="抖小云Logo" class="logo-img" />
          </div>
          <div class="brand-copy">
            <span class="eyebrow">首次运行</span>
            <h1 class="page-title">抖小云初始化配置信息</h1>
            <p class="page-desc">完成数据库、抖音账号与存储目录配置后即可进入系统。</p>
          </div>
        </div>
      </div>
    </div>

    <div class="config-card">
      <a-form ref="formModel" :model="form" :rules="formRules" :label-col="{ span: 4 }" :wrapper-col="{ span: 20 }" layout="horizontal" class="config-form" @submit.prevent="submitConfig">

        <section class="form-section database-section">
          <div class="section-heading">
            <span class="section-icon blue"><CloudServerOutlined /></span>
            <div><h3>数据持久化</h3><p>选择适合当前部署环境的数据库类型</p></div>
            <a-tag color="blue">第 1 步</a-tag>
          </div>
          <div class="section-content">
            <DatabaseConfigFields
              v-model:database-type="form.databaseType"
              v-model:host="form.databaseHost"
              v-model:port="form.databasePort"
              v-model:user-name="form.databaseUserName"
              v-model:password="form.databasePassword"
              v-model:database-name="form.databaseName"
            />
          </div>
        </section>

        <section class="form-section">
          <div class="section-heading">
            <span class="section-icon violet"><KeyOutlined /></span>
            <div><h3>抖音账号</h3><p>用于识别账号并获取需要同步的内容</p></div>
            <a-tag color="purple">第 2 步</a-tag>
          </div>
          <div class="section-content">
            <!-- Cookie名称 -->
            <a-form-item label="Cookie名" name="userName">
              <a-input v-model:value="form.userName" placeholder="例如：我的抖音账号" @input="() => {}" />
            </a-form-item>

            <!-- Cookie值 -->
            <a-form-item label="Cookie值" name="cookies">
              <div class="cookie-wrapper">
                <a-input v-model:value="form.cookies" placeholder="粘贴完整 Cookie，也可以进入系统后在“抖音授权”中补充" />
                <div class="cookie-tip">
                  <a href="https://gitee.com/deathvicky/dysync.net" target="_blank" class="cookie-link">
                    查看 Cookie 和 secUserId 获取方式 →
                  </a>
                </div>
              </div>
            </a-form-item>

            <!-- secUserId -->
            <a-form-item label="我的 secUserId" name="secUserId">
              <div class="input-with-tip">
                <a-input v-model:value="form.secUserId" placeholder="同步喜欢视频和关注博主视频时需要填写" @input="() => {}" />
                <a-tooltip title="同步喜欢视频和关注博主视频时必填">
                  <ExclamationCircleOutlined class="warning-icon" />
                </a-tooltip>
              </div>
            </a-form-item>
          </div>
        </section>

        <section class="form-section">
          <div class="section-heading">
            <span class="section-icon cyan"><FolderOpenOutlined /></span>
            <div><h3>存储策略</h3><p>设置视频下载后的持久化目录</p></div>
            <a-tag color="cyan">第 3 步</a-tag>
          </div>
          <div class="section-content">
            <div class="path-grid">
              <a-form-item label="视频存储路径" name="savePath">
                <div class="input-with-tip">
                  <a-input v-model:value="form.savePath" placeholder="例如：/app/collect" @input="() => {}" />
                  <a-tooltip title="此项必填">
                    <ExclamationCircleOutlined class="warning-icon" />
                  </a-tooltip>
                </div>
              </a-form-item>
            </div>
            <div class="storage-path-tip">初始化时所有视频共用此路径；进入系统后，可在“抖音授权”页面为喜欢和关注视频单独设置路径。</div>
          </div>
        </section>

        <!-- 操作按钮 -->
        <a-form-item :wrapper-col="{ span: 24 }" class="form-actions">
          <div class="btn-center-wrapper">
            <a-button type="primary" html-type="submit" :loading="formLoading" class="save-btn">
              <template #icon>
                <SaveOutlined />
              </template>保存配置并进入系统
            </a-button>
          </div>
        </a-form-item>
      </a-form>
    </div>
  </div>
</template>

<style scoped>
.init-config-page {
  min-height: 100vh;
  background-color: #f5f7fa;
  padding: 10px 50px;
  box-sizing: border-box;
}
.page-header {
  margin-bottom: 10px;
  width: 100%;
  display: flex;
  justify-content: center;
  align-items: center;
}
.header-content {
  text-align: center;
  max-width: 800px;
  width: 100%;
}
.logo-container {
  margin-bottom: 16px;
  display: flex;
  justify-content: center;
  align-items: center;
}
.logo-img {
  width: 60px;
  height: 60px;
  object-fit: contain;
  border-radius: 50%;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}
.page-title {
  font-size: 28px;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 8px 0;
}
.page-desc {
  font-size: 16px;
  color: #6b7280;
  margin: 0;
}
.config-card {
  background: #ffffff;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  padding: 20px;
  max-width: 1000px;
  margin: 0 auto;
}
.config-form {
  --ant-form-item-margin-bottom: 24px;
}
.input-with-tip {
  display: flex;
  align-items: center;
  width: 100%;
}
.input-with-tip :deep(.ant-input) {
  min-width: 0;
  flex: 1;
}
.form-actions {
  margin-top: 30px !important;
  margin-bottom: 0 !important;
}
.btn-center-wrapper {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  gap: 16px;
}
.save-btn {
  padding: 0 40px;
  height: 44px;
  border-radius: 8px;
  font-size: 16px;
  background-color: #1677ff;
  border-color: #1677ff;
}
.save-btn:hover {
  background-color: #4096ff;
  border-color: #4096ff;
}

/* 文本域恢复默认，让rows生效 */
:deep(.ant-input-textarea) {
  font-size: 15px;
  height: auto !important; /* 取消固定高度 */
  min-height: auto !important;
}

/* Cookie容器和链接样式 */
.cookie-wrapper {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}
.cookie-tip {
  text-align: left;
}
.cookie-link {
  color: #1677ff;
  font-size: 13px;
  text-decoration: none;
}
.cookie-link:hover {
  color: #4096ff;
  text-decoration: underline;
}

@media (max-width: 768px) {
  .init-config-page {
    padding: 10px 16px;
  }
  .config-card {
    padding: 20px;
  }
  .logo-img {
    width: 60px;
    height: 60px;
  }
  .page-title {
    font-size: 24px;
  }
  .switch-with-desc {
    flex-direction: row;
    align-items: center;
    gap: 8px;
  }
  .switch-desc {
    gap: 4px;
  }
}

/* 滚动条样式 */
:deep(.ant-input-textarea-input) {
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(150, 150, 150, 0.2) transparent;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-track {
  background: transparent;
}
:deep(.ant-input-textarea-input)::-webkit-scrollbar-thumb {
  background: rgba(150, 150, 150, 0.2);
  border-radius: 3px;
}
:deep(.ant-input-disabled) {
  background-color: #f9fafb !important;
  color: #6b7280 !important;
  cursor: not-allowed;
}

/* 初始化页视觉层级 */
.init-config-page {
  position: relative;
  overflow: hidden;
  overflow-x: clip;
  width: 100%;
  max-width: 100vw;
  padding: 10px 20px 20px;
  background:
    radial-gradient(circle at 8% 4%, rgba(37, 99, 235, 0.12), transparent 26%),
    radial-gradient(circle at 92% 20%, rgba(139, 92, 246, 0.1), transparent 24%),
    #f4f7fb;
}
.page-header {
  position: relative;
  z-index: 1;
  box-sizing: border-box;
  width: 100%;
  max-width: 1040px;
  margin: 0 auto;
  padding: 12px 20px;
  overflow: hidden;
  border: 1px solid #dfe8f5;
  border-radius: 18px;
  background:
    radial-gradient(circle at 92% 10%, rgba(96, 165, 250, 0.16), transparent 29%),
    linear-gradient(135deg, #ffffff 0%, #f5f9ff 65%, #edf4ff 100%);
  box-shadow: 0 10px 30px rgba(30, 64, 120, 0.07);
}
.page-header::after {
  display: none;
}
.header-content {
  position: relative;
  z-index: 1;
  max-width: none;
  text-align: left;
}
.brand-row {
  display: flex;
  align-items: center;
  gap: 12px;
}
.logo-container {
  flex: 0 0 auto;
  margin: 0;
  padding: 4px;
  border: 1px solid #dce7f7;
  border-radius: 16px;
  background: #fff;
  box-shadow: 0 5px 16px rgba(43, 91, 165, 0.1);
}
.logo-img {
  display: block;
  width: 42px;
  height: 42px;
  border-radius: 11px;
  box-shadow: none;
}
.brand-copy { min-width: 0; }
.eyebrow {
  display: block;
  margin-bottom: 1px;
  color: #2f6fd5;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.18em;
}
.page-title {
  margin: 0 0 2px;
  color: #17213a;
  font-size: clamp(21px, 3vw, 25px);
  font-weight: 700;
  letter-spacing: -0.025em;
}
.page-desc {
  margin: 0;
  color: #758198;
  font-size: 13px;
}
.config-card {
  position: relative;
  z-index: 2;
  box-sizing: border-box;
  width: 100%;
  max-width: 1040px;
  margin: 8px auto 0;
  padding: 12px;
  border: 1px solid rgba(219, 227, 239, 0.85);
  border-radius: 20px;
  box-shadow: 0 12px 34px rgba(15, 35, 75, 0.075);
}
.form-section {
  margin-bottom: 8px;
  overflow: hidden;
  border: 1px solid #e7ecf4;
  border-radius: 14px;
  background: #fff;
  transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
.config-form,
.form-section,
.section-content,
.path-grid,
.section-heading > div {
  min-width: 0;
}
.path-grid {
  display: block;
}
.storage-path-tip {
  margin: -3px 0 10px;
  padding-left: 16.6667%;
  color: #7f899c;
  font-size: 12px;
  line-height: 1.5;
}
.form-section:hover {
  border-color: #cfdaeb;
  box-shadow: 0 8px 24px rgba(34, 62, 110, 0.055);
}
.section-heading {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 14px;
  border-bottom: 1px solid #edf1f6;
  background: #fbfcfe;
}
.section-heading > div { flex: 1; }
.section-heading h3 { margin: 0; color: #202a42; font-size: 14px; font-weight: 650; }
.section-heading p { margin: 0; color: #8a94a7; font-size: 12px; }
.section-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 29px;
  height: 29px;
  border-radius: 10px;
  font-size: 17px;
}
.section-icon.blue { color: #2563eb; background: #eaf2ff; }
.section-icon.violet { color: #7c3aed; background: #f2ebff; }
.section-icon.cyan { color: #0891b2; background: #e8f8fb; }
.section-content { padding: 10px 16px 0; }
:deep(.section-content .ant-form-item) { margin-bottom: 10px; }
:deep(.database-section .database-tip) { margin: 3px 0 8px; line-height: 1.35; }
.warning-icon { margin-left: 8px; color: #f59e0b; font-size: 16px; }
:deep(.section-content .ant-form-item-label > label) { color: #4c5870; font-weight: 500; }
:deep(.section-content .ant-input),
:deep(.section-content .ant-input-affix-wrapper),
:deep(.section-content .ant-input-number),
:deep(.section-content .ant-select-selector) {
  border-radius: 8px !important;
}
:deep(.section-content input.ant-input),
:deep(.section-content .ant-input-affix-wrapper),
:deep(.section-content .ant-input-number),
:deep(.section-content .ant-select-selector) {
  min-height: 34px;
}
:deep(.section-content .ant-input:focus),
:deep(.section-content .ant-input-affix-wrapper-focused),
:deep(.section-content .ant-select-focused .ant-select-selector) {
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.09) !important;
}
.form-actions { margin-top: 2px !important; }
.save-btn {
  min-width: 240px;
  height: 40px;
  border: 0;
  border-radius: 10px;
  background: linear-gradient(100deg, #2563eb, #3779ef);
  box-shadow: 0 9px 20px rgba(37, 99, 235, 0.22);
  font-weight: 600;
}
.save-btn:hover {
  background: linear-gradient(100deg, #1d4ed8, #2f6fe1);
  box-shadow: 0 11px 24px rgba(37, 99, 235, 0.28);
  transform: translateY(-1px);
}

@media (max-width: 768px) {
  .init-config-page { padding: 8px 8px 18px; }
  .page-header { padding: 11px 12px; border-radius: 13px; }
  .brand-row { align-items: flex-start; gap: 14px; }
  .logo-container { padding: 5px; border-radius: 15px; }
  .logo-img { width: 52px; height: 52px; border-radius: 11px; }
  .page-title { margin-top: 2px; font-size: 25px; }
  .page-desc { font-size: 13px; line-height: 1.6; }
  .eyebrow { display: none; }
  .config-card { margin-top: 7px; padding: 8px; border-radius: 13px; }
  .section-heading { padding: 8px 10px; }
  .section-heading :deep(.ant-tag) { display: none; }
  .section-content { padding: 9px 9px 0; }
  :deep(.config-form .ant-form-item-label) { padding-bottom: 5px; text-align: left; }
  :deep(.config-form .ant-form-item-label),
  :deep(.config-form .ant-form-item-control) { flex: 0 0 100%; max-width: 100%; }
  .save-btn { width: 100%; min-width: 0; }
  .storage-path-tip { padding-left: 0; }
}
</style>
