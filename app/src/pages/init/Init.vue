<template>
  <a-card :bordered="false" :bodyStyle='{"padding-top":"0px","padding-bottom":"100px"}'>
    <a-form :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef">
      <a-divider orientation="left"></a-divider>

      <a-form-item has-feedback label="同步周期(分钟)" ref="Cron" name="Cron">
        <a-input v-model:value="formState.Cron" placeholder="1：数字-例如20-表示20分钟执行一次；2：cron表达式，根据表达式周期执行" />
      </a-form-item>
      <a-form-item label="在线Cron表达式">
        <a target="_blank" href="https://www.bejson.com/othertools/cron/">查看示例</a>
      </a-form-item>

      <a-form-item :wrapper-col="{ span: 10, offset: 3 }">
        <a-button type="primary" danger @click="onSubmit">进入系统</a-button>
      </a-form-item>
    </a-form>
  </a-card>
</template>
<script lang="ts" setup>
import { reactive, toRaw, ref, watch, createVNode, h } from 'vue';
import type { UnwrapRef } from 'vue';
import { Form } from 'ant-design-vue';
import type { Rule } from 'ant-design-vue/es/form';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { Modal } from 'ant-design-vue';
import { checkDomain, checksubDomainPrefix, checkPass, checkUserName } from '@/utils/regexHelper';
const formRef = ref<FormInstance>();
const SK = ref(null);
interface FormState {
  cloudName: string;
  domainName: string;
  recordType: string;
  ipv6Prefix: string;
  domainRecord: string;
  AK: string;
  SK: string;
  DbType: number;
  ConnString: string;
  Cron: string;
  UserName: string;
  UswePwd: string;
}
interface CloudInfo {
  key: string;
  value: string;
  doc: string;
}
interface DbTypeInfo {
  key: number;
  value: string;
  conn: string;
}
interface showDBconn {
  value: boolean;
}

const showDBconn: UnwrapRef<showDBconn> = reactive({
  value: false,
});

const formState: UnwrapRef<FormState> = reactive({
  cloudName: 'aliyun',
  domainName: '',
  recordType: '',
  ipv6Prefix: '',
  domainRecord: '',
  AK: '',
  SK: '',
  DbType: 2,
  ConnString: '',
  Cron: '30',
  UserName: '',
  UswePwd: '',
});

const rules: Record<string, Rule[]> = {
  Cron: [{ required: true, message: '请输入任务调度周期', trigger: 'change' }],
};

watch(
  () => formState.DbType,
  () => {
    formRef.value.validateFields(['ConnString']);
  },
  { flush: 'post' }
);

//检查数据库连接
import { message } from 'ant-design-vue';
import router from '@/router';
import { ExclamationCircleOutlined } from '@ant-design/icons-vue';
const isFirst = ref(false);

//提交初始化

const onSubmit = () => {
  // console.log('submit!', toRaw(formState));
  formRef.value
    .validate()
    .then(() => {
      console.log('values', formState, toRaw(formState));
      useApiStore()
        .apiInit(toRaw(formState))
        .then((res) => {
          if (res.code === 0) {
            message.success('初始化成功');
            setTimeout(() => {
              router.push('/login');
            }, 1000);
          } else {
            message.error(res.erro, 8);
          }
        });
    })
    .catch((error) => {
      console.log('error', error);
    });
};

const labelCol = { style: { width: '150px' } };
const wrapperCol = { span: 4 };
</script>
<style scoped>
.ant-card-body {
  padding: 5px !important;
}
</style>

