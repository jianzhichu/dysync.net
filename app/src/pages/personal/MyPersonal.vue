<template>
  <a-drawer v-model:visible="visible" class="custom-class" title="个人设置" placement="right" @after-visible-change="afterVisibleChange" :maskClosable="true">
    <a-tabs v-model:activeKey="activeKey">
      <!-- <a-tab-pane key="1" style="height:100%">
        <template #tab>
          <span>
            <user-outlined />
            修改头像
          </span>
        </template>

        <a-upload style="" v-model:file-list="fileList" name="file" list-type="picture-card" class="avatar-uploader" :show-upload-list="false" :action="uploadAction" :before-upload="beforeUpload" @change="handleChange" accept=".jpg, .jpeg, .png">
          <img v-if="imageUrl" :src="imageUrl" alt="avatar" style="height: 112px; width: 112px; border-radius: 50%;" />
          <div v-else>
            <loading-outlined v-if="loading"></loading-outlined>
            <plus-outlined v-else></plus-outlined>
            <div class="ant-upload-text">选择图片</div>
          </div>
        </a-upload>
        <div style="margin-top:20px;">
          上传成功即修改成功
        </div>
      </a-tab-pane> -->
      <a-tab-pane key="2">
        <template #tab>
          <span>
            <safety-outlined />
            修改用户信息
          </span>
        </template>
        <a-form ref="passFromRef" :model="passwordFormData" name="basic" :label-col="{ span: 8 }" :wrapper-col="{ span: 16 }" autocomplete="off" @finish="onFinish" @finishFailed="onFinishFailed" validateTrigger="blur">
          <a-form-item label="原密码" ref="OldPassword" name="OldPassword" :rules="[{ required: true, message: '请输入原密码!' }]">
            <a-input-password v-model:value="passwordFormData.OldPassword" />
          </a-form-item>

          <a-form-item label="新密码" ref="Password" name="Password" :rules="[{ required: true, message: '请输入正确新密码!密码要求6位以上包含大小写与数字' ,validator:checkPassword}]">
            <a-input-password v-model:value="passwordFormData.Password" />
          </a-form-item>
          <a-form-item label="确认密码" ref="ConfirmPassword" name="ConfirmPassword" :rules="[{ required: true, message: '请输入正确新密码!',validator:checkPassword}]">
            <a-input-password v-model:value="passwordFormData.ConfirmPassword" />
          </a-form-item>
          <a-form-item label="修改账户" ref="UserName" name="UserName" :rules="[{ required: true, message: '请输入新账号!' }]">
            <a-input v-model:value="passwordFormData.UserName" />
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
import { defineComponent, ref, onMounted, reactive } from 'vue';
import type { UploadChangeParam, UploadProps, FormInstance } from 'ant-design-vue';
import { useApiStore, useAccountStore } from '@/store';
import http from '@/store/http';
import { checkPass } from '@/utils/regexHelper';
import type { Rule } from 'ant-design-vue/es/form';

const activeKey = ref<string>('2');
const uploadAction = ref<string>('');
const visible = ref<boolean>(false);

function show(vis: boolean) {
  visible.value = vis;
  if (vis) loadUserInfo();
}
const passFromRef = ref<FormInstance>();
const afterVisibleChange = (bool: boolean) => {
  if (!bool) {
    if (passFromRef != null && passFromRef.value != null) passFromRef.value.resetFields();
  }
};

function getBase64(img: Blob, callback: (base64Url: string) => void) {
  const reader = new FileReader();
  reader.addEventListener('load', () => callback(reader.result as string));
  reader.readAsDataURL(img);
}
const fileList = ref([]);
const loading = ref<boolean>(false);
const imageUrl = ref<string>('');

const handleChange = (info: UploadChangeParam) => {
  if (info.file.status === 'uploading') {
    loading.value = true;
    return;
  }
  if (info.file.status === 'done') {
    // Get this url from response in real world.
    getBase64(info.file.originFileObj, (base64Url: string) => {
      imageUrl.value = base64Url;
      loading.value = false;
    });
  }
  if (info.file.status === 'error') {
    loading.value = false;
    message.error('upload error');
  }
};

const beforeUpload = (file: UploadProps['fileList'][number]) => {
  const isJpgOrPng = file.type === 'image/jpeg' || file.type === 'image/png' || file.type === 'image/jpg';
  if (!isJpgOrPng) {
    message.error('仅允许上传jpg|png|jpeg格式');
  }
  const isLt10M = file.size / 1024 / 1024 < 10;
  if (!isLt10M) {
    message.error('最大允许上传5M的文件!');
  }
  return isJpgOrPng && isLt10M;
};

// const userInfo = ref<string>();

const loadUserInfo = () => {
  useApiStore()
    .apiUserInfo()
    .then((res) => {
      if (res.code === 0) {
        if (res.data.avatar != null && res.data.avatar !== '') imageUrl.value = `/upload/${res.data.avatar}`;
        passwordFormData.UserId = res.data.id;
        uploadAction.value = `/api/auth/UpdateUserAvatar?Uid=${res.data.id}`;
      }
    });
};

interface PassFormState {
  UserId: string;
  OldPassword: string;
  Password: string;
  ConfirmPassword: string;
  UserName: string;
}

const onFinish = (values: PassFormState) => {
  if (values.Password !== values.ConfirmPassword) {
    message.error('新密码与确认密码不一致');
    return;
  } else {
    useApiStore()
      .apiChangePwd(passwordFormData)
      .then((res) => {
        console.log(res);
        if (res.code === 0) {
          message.success('修改密码成功,请重新登陆！');
          useAccountStore().setLogged(false);
          visible.value = false;
          http.removeAuthorization();
        } else {
          message.error(res.message, 8);
        }
      });
  }
};

const checkPassword = async (_rule: Rule, value: string) => {
  if (checkPass(value)) {
    return Promise.resolve();
  } else {
    return Promise.reject('密码要求6位以上包含大小写与数字');
  }
};

const onFinishFailed = (errorInfo: any) => {
  console.log('Failed:', errorInfo);
};
const passwordFormData = reactive<PassFormState>({
  UserId: '',
  OldPassword: '',
  Password: '',
  ConfirmPassword: '',
  UserName: '',
});
// 将updateMessage方法暴露给父组件调用
defineExpose({
  show,
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