<script lang="ts" setup>
import { reactive, ref, watch, UnwrapRef, onMounted } from 'vue';
import { FormInstance } from 'ant-design-vue';
import { message } from 'ant-design-vue';
import { useApiStore, useAccountStore } from '@/store';
import { useRouter } from 'vue-router';

import { ExclamationCircleOutlined, SyncOutlined, SaveOutlined } from '@ant-design/icons-vue';

interface UpSecUserIdItem {
  uper?: string;
  uid?: string;
  syncAll: boolean;
}

type ConfigItem = {
  id?: string;
  userName: string; // 改为非可选，强制初始化
  cookies: string;
  savePath: string;
  favSavePath: string;
  secUserId: string;
  status: number;
  upSavePath: string;
  imgSavePath: string;
  useSinglePath: boolean; // 非可选
};

// 强制初始化所有字段为非undefined值
const newConfig = (config?: ConfigItem): ConfigItem => {
  return {
    id: '0',
    userName: '', // 强制空字符串
    cookies: '',
    savePath: '',
    favSavePath: '',
    secUserId: '',
    status: 0,
    upSavePath: '',
    imgSavePath: '',
    useSinglePath: false,
  };
};

// 改用 ref 而非 reactive（解决深层响应式问题）
const form = ref<ConfigItem>(newConfig());
const formModel = ref<FormInstance>();
const formLoading = ref(false);

// 重置表单：强制重置 + 清空校验状态
const resetConfig = () => {
  form.value = newConfig();
  formModel.value?.resetFields(); // 必须调用，清空校验提示
  message.info('表单已重置为默认值');
};

// 监听统一路径（改用 ref 的取值方式）
watch(
  [() => form.value.savePath, () => form.value.useSinglePath],
  ([newSavePath, useSinglePath]) => {
    if (useSinglePath && newSavePath) {
      form.value.favSavePath = newSavePath;
      form.value.upSavePath = newSavePath;
      form.value.imgSavePath = newSavePath;
    }
  },
  { immediate: true }
);

// 校验规则：明确trigger + 不依赖组件自动触发
const formRules = reactive({
  userName: [{ required: true, message: '请输入Cookie名称', trigger: 'change' }],
  savePath: [{ required: true, message: '请输入收藏存储路径', trigger: 'change' }],
  secUserId: [{ required: false, message: '请输入我的secUserId', trigger: 'change' }],
});

const router = useRouter();
// 定义rowCount并暴露到模板（之前定义在onMounted里，模板访问不到）
const rowCount = ref(6);

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

const accountStore = useAccountStore();

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
      message.success('配置保存成功,即将跳转登录页...');
      setTimeout(() => {
        router.push('/login');
      }, 500);
    } else {
      message.error(`保存失败：${res.message || '未知错误'}`);
    }
  } catch (err: any) {
    console.error('校验失败：', err);
    const firstErr = err.errorFields?.[0]?.errors?.[0] || '请检查所有必填项';
    message.error(firstErr);
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
    return { pass: false, msg: '请输入收藏存储路径' };
  }
  return { pass: true, msg: '' };
};
</script>

<template>
  <div class="init-config-page">
    <div class="page-header">
      <div class="header-content">
        <div class="logo-container">
          <img src="/logo.png" alt="抖小云Logo" class="logo-img" />
        </div>
        <h1 class="page-title">抖小云初始化配置信息</h1>
        <!-- <p class="page-desc">第一次运行，请花两分钟完成初始配置</p> -->
      </div>
    </div>

    <div class="config-card">
      <a-form ref="formModel" :model="form" :rules="formRules" :label-col="{ span: 4 }" :wrapper-col="{ span: 20 }" layout="horizontal" class="config-form" @submit.prevent="submitConfig">

        <!-- Cookie名称 -->
        <a-form-item label="Cookie名" name="userName">
          <a-input v-model:value="form.userName" placeholder="随便填个名称,作为唯一标识" @input="() => {}" />
        </a-form-item>

        <!-- Cookie值：现在rows能正常生效了 -->
        <a-form-item label="Cookie值" name="cookies">
          <div class="cookie-wrapper">
            <a-textarea v-model:value="form.cookies" :rows="rowCount" placeholder="粘贴完整Cookie值，也可以先不填，保存配置后，后续可以在“抖音授权”修改" />
            <div class="cookie-tip">
              <a href="https://gitee.com/deathvicky/dysync.net" target="_blank" class="cookie-link">
                查看Cookie以及secUserId获取方式
              </a>
            </div>
          </div>
        </a-form-item>

        <!-- secUserId -->
        <a-form-item label="我的secUserId" name="secUserId">
          <div class="input-with-tip">
            <a-input v-model:value="form.secUserId" placeholder="同步喜欢视频和关注博主视频必填" @input="() => {}" />
            <a-tooltip title="同步喜欢视频和关注博主视频必填">
              <ExclamationCircleOutlined style="color: #faad14; margin-left: 8px; font-size: 16px;" />
            </a-tooltip>
          </div>
        </a-form-item>

        <!-- 收藏路径 -->
        <a-form-item label="收藏存储路径" name="savePath">
          <div class="input-with-tip">
            <a-input v-model:value="form.savePath" placeholder="/app/collect" @input="() => {}" />
            <a-tooltip title="必填">
              <ExclamationCircleOutlined style="color: #faad14;font-size: 16px; margin-left: 8px; " />
            </a-tooltip>
          </div>
        </a-form-item>

        <!-- 统一路径开关 -->
        <a-form-item label="统一存储路径" name="useSinglePath">
          <div class="switch-with-desc">
            <a-switch v-model:checked="form.useSinglePath" class="custom-large-switch" @change="() => {}" />
            <div class="switch-desc">
              <span class="main-desc">{{ form.useSinglePath ? '开启' : '关闭' }}</span>
              <span class="sub-desc">{{ form.useSinglePath ? '所有类型视频(收藏的、喜欢的、关注博主的)共用一个路径' : '各类型视频(收藏的、喜欢的、关注博主的)文件路径独立配置' }}</span>
            </div>
          </div>
        </a-form-item>
        <!-- 其他路径 -->
        <a-form-item v-if="!form.useSinglePath" label="喜欢存储路径" name="favSavePath">
          <a-input v-model:value="form.favSavePath" placeholder="喜欢视频存储路径，不想同步就空着，后续可以在“抖音授权”修改" @input="() => {}" />
        </a-form-item>

        <a-form-item v-if="!form.useSinglePath" label="关注存储路径" name="upSavePath">
          <a-input v-model:value="form.upSavePath" placeholder="关注视频存储路径，不想同步就空着，后续可以在“抖音授权”修改" @input="() => {}" />
        </a-form-item>

        <a-form-item v-if="!form.useSinglePath" label="图文存储路径" name="imgSavePath">
          <a-input v-model:value="form.imgSavePath" placeholder="图文视频存储路径，不想同步就空着，后续可以在“抖音授权”修改" @input="() => {}" />
        </a-form-item>

        <!-- 同步状态开关 -->
        <a-form-item label="同步状态" name="status">
          <div class="switch-with-desc">
            <a-switch v-model:checked="form.status" :checked-value="1" :un-checked-value="0" class="custom-large-switch" />
            <div class="switch-desc">
              <span class="main-desc">{{ form.status === 1 ? '开启' : '关闭' }}</span>
              <span class="sub-desc">建议初始不开启，保存设置后，进入系统配置完成其他设置后再手动开启！</span>
            </div>
          </div>
        </a-form-item>

        <!-- 操作按钮 -->
        <a-form-item :wrapper-col="{ span: 24 }" class="form-actions">
          <div class="btn-center-wrapper">
            <a-button type="primary" html-type="submit" :loading="formLoading" class="save-btn">
              <template #icon>
                <SaveOutlined />
              </template>保存配置
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
.switch-with-desc {
  display: flex;
  align-items: center;
  gap: 12px;
}
.switch-desc {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}
.switch-desc .main-desc {
  font-size: 15px;
  color: #374151;
  font-weight: 500;
}
.switch-desc .sub-desc {
  font-size: 13px;
  color: #6b7280;
}
:deep(.custom-large-switch) {
  transform: scale(1.2);
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
  :deep(.custom-large-switch) {
    transform: scale(1.1);
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
</style>