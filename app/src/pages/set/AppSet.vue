<template>
  <a-card :bordered="false" :bodyStyle='{}'>
    <a-form :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef">

      <a-form-item has-feedback label="同步周期(分钟)" ref="Cron" name="Cron">
        <a-input v-model:value="formState.Cron" placeholder="" />
        <a-alert message="1：数字 20-表示20分钟执行一次；" type="success" />
        <a-alert message="2：cron表达式，根据表达式周期执行" type="success" />
      </a-form-item>
      <a-form-item label="在线Cron表达式">
        <a target="_blank" href="https://www.bejson.com/othertools/cron/">查看示例</a>
      </a-form-item>

      <a-form-item has-feedback label="扫描行数" ref="BatchCount" name="BatchCount">
        <a-input v-model:value="formState.BatchCount" placeholder="" />
        <a-alert message="每次扫描行数，第一次同步完成后，可以适当调高提高效率" type="success" />
      </a-form-item>

      <a-form-item :wrapper-col="{ span: 10, offset: 3 }">
        <a-space>
          <a-button type="primary" @click="onUpdate" v-if="componentDisabled">修改配置</a-button>
          <a-button type="primary" danger @click="onSubmit" v-if="!componentDisabled">确认</a-button>
          <a-button type="default" @click="onCancel" v-if="!componentDisabled">取消</a-button>
        </a-space>
      </a-form-item>
    </a-form>

  </a-card>
</template>
<script lang="ts" setup>
import { reactive, toRaw, ref, watch } from 'vue';
import type { UnwrapRef } from 'vue';
import { Form } from 'ant-design-vue';
import type { Rule } from 'ant-design-vue/es/form';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { message } from 'ant-design-vue';
import { onMounted } from 'vue';
const formRef = ref<FormInstance>();
const componentDisabled = ref(true);
const SK = ref(null);
interface FormState {
  Cron: string;
  Id: string;
  BatchCount: number;
}
const formState: UnwrapRef<FormState> = reactive({
  Cron: '30',
  Id: '0',
  BatchCount: 10,
});

const rules: Record<string, Rule[]> = {
  Cron: [{ required: true, message: '请输入任务调度周期', trigger: 'change' }],
};

const getConfig = () => {
  useApiStore()
    .apiGetConfig()
    .then((res) => {
      if (res.code === 0) {
        formState.Cron = res.data.cron;
        formState.Id = res.data.id;
        formState.BatchCount = res.data.batchCount;
      } else {
        message.error(res.erro, 8);
      }
    });
};
onMounted(() => {
  getConfig();
});

//提交初始化

const onSubmit = () => {
  // console.log('submit!', toRaw(formState));
  formRef.value
    .validate()
    .then(() => {
      console.log('values', formState, toRaw(formState));
      useApiStore()
        .apiUpdateConfig(toRaw(formState))
        .then((res) => {
          if (res.code === 0) {
            message.success('配置修改生效，同步任务将按照新的规则执行');
            componentDisabled.value = true;
          } else {
            message.error(res.erro, 8);
          }
        });
    })
    .catch((error) => {
      console.log('error', error);
    });
};

//修改配置开启
const onUpdate = () => {
  componentDisabled.value = false;
};
const onCancel = () => {
  componentDisabled.value = true;
};
const labelCol = { style: { width: '150px' } };
const wrapperCol = { span: 4 };
</script>

<style lang='less' scoped>
.ant-radio-button-wrapper-disabled.ant-radio-button-wrapper-checked {
  color: rgb(164 158 158) !important;
  background-color: #e6e6e6 !important;
}
</style>