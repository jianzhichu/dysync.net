<template>
  <a-drawer v-model:visible="visible" class="custom-class" title="邮件配置" placement="right" @after-visible-change="afterVisibleChange" :maskClosable="true">
    <a-tabs v-model:activeKey="activeKey">
      <a-tab-pane key="1">
        <template #tab>
          <span>
            <!-- <BellOutlined /> -->
            邮件STMP配置
          </span>
        </template>
        <a-form ref="emailFromRef" :model="EmailFormData" name="basic" :label-col="{ span: 8 }" :wrapper-col="{ span: 16 }" autocomplete="off" @finish="onFinish" @finishFailed="onFinishFailed">
          <a-form-item label="是否开启" ref="Open">
            <a-checkbox v-model:checked="EmailFormData.Open">解析结果邮件通知</a-checkbox>
          </a-form-item>
          <a-form-item v-if="EmailFormData.Open" label="Stmp地址" ref="Stmp" name="Stmp" :rules="[{ required: EmailFormData.Open, message: '请输入stmp服务地址!',validator:validateStmp }]">
            <a-input v-model:value="EmailFormData.Stmp" />
          </a-form-item>
          <a-form-item v-if="EmailFormData.Open" label="Stmp端口" ref="Port" name="Port" :rules="[{ required: EmailFormData.Open, message: '请输入stmp服务端口!',validator:validatePort }]">
            <a-input v-model:value="EmailFormData.Port" placeholder="默认465，yeah-587" />
          </a-form-item>
          <a-form-item v-if="EmailFormData.Open" label="收件Email" type='email' ref="From" name="From" :rules="[{ required: EmailFormData.Open, message: '请输入收件Email地址!',validator:validateEmail }]">
            <a-input v-model:value="EmailFormData.From" />
            <span style="color:#888">此处收件人亦是发件人</span>
          </a-form-item>
          <a-form-item v-if="EmailFormData.Open" label="Stmp授权码" ref="Code" name="Code" :rules="[{ required: EmailFormData.Open, message: '请输入Stmp授权码!' }]">
            <a-input v-model:value="EmailFormData.Code" />
            <span style="color:#888">邮箱设置开启stmp服务会提示</span>
          </a-form-item>
          <a-form-item :wrapper-col="{ offset: 8, span: 16 }">
            <a-button type="primary" html-type="submit">确认</a-button>
          </a-form-item>
        </a-form>
      </a-tab-pane>
    </a-tabs>
  </a-drawer>
</template>
<script lang="ts" setup>
import { message } from 'ant-design-vue';
import { defineComponent, ref, onMounted, reactive, watch } from 'vue';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore, useAccountStore } from '@/store';
import http from '@/store/http';
import type { Rule } from 'ant-design-vue/es/form';
import { checkEmail, checkDomain, checkPort } from '@/utils/regexHelper';
const activeKey = ref<string>('1');
const visible = ref<boolean>(false);

function showEmail(vis: boolean) {
  visible.value = vis;
  if (vis) loadStmpInfo();
}
const emailFromRef = ref<FormInstance>();
const afterVisibleChange = (bool: boolean) => {
  if (!bool) {
    emailFromRef.value.resetFields();
  }
};

const loadStmpInfo = () => {
  // useApiStore()
  //   .apiGetStmpConfig()
  //   .then((res) => {
  //     if (res.code === 0) {
  //       EmailFormData.Open = res.data.open;
  //       EmailFormData.Stmp = res.data.stmp;
  //       EmailFormData.Port = res.data.port;
  //       EmailFormData.Code = res.data.code;
  //       EmailFormData.From = res.data.from;
  //       EmailFormData.Id = res.data.id;
  //     }
  //   });
};

interface EmailFormState {
  Open: boolean;
  Stmp: string;
  Port: number;
  Code: string;
  From: string;
  Id: string;
  To: string;
}
const EmailFormData = reactive<EmailFormState>({
  Open: false,
  Stmp: 'smtp.qq.com',
  Port: 465,
  Code: '',
  From: '',
  Id: '',
  To: '',
});
const onFinish = (values: EmailFormState) => {
  EmailFormData.To = EmailFormData.From;
  // useApiStore()
  //   .apiSaveStmpConfig(EmailFormData)
  //   .then((res) => {
  //     console.log(res);
  //     if (res.code === 0) {
  //       message.success('修改配置成功');
  //       visible.value = false;
  //     } else {
  //       message.error(res.erro, 8);
  //     }
  //   });
};

//验证邮箱
const validateEmail = async (_rule: Rule, value: string) => {
  if (checkEmail(value)) {
    return Promise.resolve();
  } else {
    return Promise.reject('请输入正确的邮箱地址');
  }
};

//验证stmp服务
const validateStmp = async (_rule: Rule, value: string) => {
  if (checkDomain(value)) {
    return Promise.resolve();
  } else {
    return Promise.reject('请输入正确的stmp-server地址');
  }
};
//验证端口
const validatePort = async (_rule: Rule, value: string) => {
  if (checkPort(value)) {
    return Promise.resolve();
  } else {
    return Promise.reject('请输入正确的端口');
  }
};
const onFinishFailed = (errorInfo: any) => {
  console.log('Failed:', errorInfo);
};

watch(
  () => EmailFormData.Open,
  () => {
    emailFromRef.value.validateFields(['Stmp', 'Port', 'From', 'Code']);
  },
  { flush: 'post' }
);

// 将updateMessage方法暴露给父组件调用
defineExpose({
  showEmail,
});
</script>

<style>
.avatar-uploader > .ant-upload {
  width: 100px;
  height: 100px;
}
.ant-upload-picture-card-wrapper {
  height: 100%;
}
.ant-upload-select-picture-card i {
  font-size: 32px;
  color: #999;
  margin-top: 50px !important;
}
.ant-upload-select-picture-card {
  margin-top: 50px !important;
}
.ant-upload-select-picture-card .ant-upload-text {
  margin-top: 8px;
  color: #666;
}
.ant-tabs-content {
  text-align: center;
}
</style>