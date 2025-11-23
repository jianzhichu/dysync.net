<template>
  <a-card :bordered="false" :body-style="{ padding: '20px' }">
    <a-form :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef" label-align="right">
      <!-- 任务调度配置 -->
      <div class="form-section">
        <h3 class="section-title">任务调度配置</h3>

        <a-form-item has-feedback label="同步周期(分钟)" name="Cron" :wrapper-col="{ span: 6 }">
          <a-input v-model:value="formState.Cron" placeholder="请输入数字或Cron表达式" />
          <div class="help-text">
            <p>1. 数字：表示每隔该分钟执行一次（如 20 表示每20分钟）</p>
            <p>2. Cron表达式：按表达式规则执行（如 0 0/1 * * * 表示每小时执行一次）</p>
          </div>
          <a-form-item label="在线Cron表达式" :wrapper-col="{ span: 12, offset: 0 }">
            <a target="_blank" href="https://www.bejson.com/othertools/cron/" class="cron-link">查看示例</a>
          </a-form-item>
        </a-form-item>

      </div>

      <!-- 文件保存配置 -->
      <div class="form-section">
        <h3 class="section-title">博主视频保存配置</h3>

        <a-form-item has-feedback label="用标题做文件名" name="UperUseViedoTitle" :wrapper-col="{ span: 6 }">
          <a-switch v-model:checked="formState.UperUseViedoTitle" />
        </a-form-item>

        <a-form-item has-feedback label="不创建子文件夹" name="UperSaveTogether" :wrapper-col="{ span: 6 }">
          <a-switch v-model:checked="formState.UperSaveTogether" />
        </a-form-item>

      </div>

      <!-- 系统配置 -->
      <div class="form-section">
        <h3 class="section-title">系统配置</h3>
        <a-form-item has-feedback label="每次同步条数" name="BatchCount" :wrapper-col="{ span: 12 }">
          <a-input-number v-model:value="formState.BatchCount" placeholder="请输入查询条数" :min="1" :max="100" />
        </a-form-item>
        <a-form-item has-feedback label="日志保留天数" name="LogKeepDay" :wrapper-col="{ span: 12 }">
          <a-input-number v-model:value="formState.LogKeepDay" placeholder="请输入保留天数" :min="1" :max="90" />
        </a-form-item>
        <a-form-item v-if="downImgVideo" has-feedback label="同步图文视频" name="DownImageVideo" :wrapper-col="{ span: 6 }">
          <a-switch v-model:checked="formState.DownImageVideo" />
        </a-form-item>

      </div>

      <!-- 操作按钮 -->
      <a-form-item :wrapper-col="{ span: 12, offset: 5 }" class="form-actions">
        <a-space size="middle">
          <a-button type="primary" @click="onUpdate" v-if="componentDisabled">修改配置</a-button>
          <a-button type="primary" danger @click="onSubmit" v-if="!componentDisabled">确认保存</a-button>
          <a-button type="default" @click="onCancel" v-if="!componentDisabled">取消</a-button>
        </a-space>
      </a-form-item>
    </a-form>
  </a-card>
</template>

<script lang="ts" setup>
import { reactive, toRaw, ref, watch, onMounted } from 'vue';
import type { UnwrapRef } from 'vue';
import { Form } from 'ant-design-vue';
import type { Rule } from 'ant-design-vue/es/form';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { message } from 'ant-design-vue';

// 表单引用
const formRef = ref<FormInstance>();

// 状态定义
const componentDisabled = ref(true);
const downImgVideo = ref(false);

// 表单数据结构
interface FormState {
  Cron: string;
  Id: string;
  BatchCount: number;
  DownImageVideoFromEnv: boolean;
  DownImageVideo: boolean;
  UperSaveTogether: boolean;
  UperUseViedoTitle: boolean;
  LogKeepDay: number;
}

// 表单初始数据
const formState: UnwrapRef<FormState> = reactive({
  Cron: '30',
  Id: '0',
  BatchCount: 10,
  LogKeepDay: 10,
  UperUseViedoTitle: false,
  UperSaveTogether: false,
  DownImageVideo: false,
  DownImageVideoFromEnv: false,
});

// 表单校验规则
const rules: Record<string, Rule[]> = {
  Cron: [
    { required: true, message: '请输入任务调度周期', trigger: 'change' },
    {
      validator: (rule, value) => {
        // 验证数字或cron表达式
        if (/^\d+$/.test(value) || /^(\d+\s*){5,6}$/.test(value)) {
          return Promise.resolve();
        }
        return Promise.reject(new Error('请输入有效的数字或Cron表达式'));
      },
      trigger: 'change',
    },
  ],
  BatchCount: [
    { required: true, message: '请输入每次查询条数', trigger: 'change' },
    { type: 'number', min: 1, max: 100, message: '查询条数应在1-100之间', trigger: 'change' },
  ],
  LogKeepDay: [
    { required: true, message: '请输入日志保留天数', trigger: 'change' },
    { type: 'number', min: 1, max: 90, message: '保留天数应在1-90之间', trigger: 'change' },
  ],
};

// 布局配置
const labelCol = { style: { width: '150px', textAlign: 'right' } };
const wrapperCol = { span: 12 };

// 获取配置数据
const getConfig = () => {
  useApiStore()
    .apiGetConfig()
    .then((res) => {
      if (res.code === 0) {
        // 使用Object.assign避免重复赋值
        Object.assign(formState, {
          Cron: res.data.cron,
          Id: res.data.id,
          BatchCount: res.data.batchCount,
          DownImageVideoFromEnv: res.data.downImageVideoFromEnv,
          DownImageVideo: res.data.downImageVideo,
          UperUseViedoTitle: res.data.uperUseViedoTitle,
          UperSaveTogether: res.data.uperSaveTogether,
          LogKeepDay: res.data.logKeepDay,
        });

        // 监听环境变量配置，控制是否显示图片视频下载选项
        downImgVideo.value = res.data.downImageVideoFromEnv;
      } else {
        message.error(res.erro || '获取配置失败', 8);
      }
    })
    .catch((error) => {
      console.error('获取配置失败:', error);
      message.error('获取配置失败，请稍后重试', 8);
    });
};

// 组件挂载时获取配置
onMounted(() => {
  getConfig();
});

// // 监听环境变量变化
// watch(
//   () => formState.DownImageVideoFromEnv,
//   (value) => {
//     downImgVideo.value = value;
//   }
// );

// 提交表单
const onSubmit = () => {
  formRef.value
    .validate()
    .then(() => {
      useApiStore()
        .apiUpdateConfig(toRaw(formState))
        .then((res) => {
          if (res.code === 0) {
            message.success('配置修改生效，同步任务将按照新的规则执行');
            componentDisabled.value = true;
          } else {
            message.error(res.erro || '更新配置失败', 8);
          }
        })
        .catch((error) => {
          console.error('更新配置失败:', error);
          message.error('更新配置失败，请稍后重试', 8);
        });
    })
    .catch((error) => {
      console.log('表单校验失败:', error);
      message.error('请检查表单填写是否正确', 8);
    });
};

// 进入编辑模式
const onUpdate = () => {
  componentDisabled.value = false;
};

// 取消编辑
const onCancel = () => {
  componentDisabled.value = true;
  // 可以考虑添加重置表单的逻辑
  formRef.value?.clearValidate();
};
</script>

<style lang='less' scoped>
@primary-color: #1890ff;
@text-color: #333;
@border-radius: 4px;

.form-section {
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #f0f0f0;

  &:last-child {
    border-bottom: none;
  }
}

.section-title {
  font-size: 16px;
  font-weight: 500;
  color: @primary-color;
  margin-bottom: 15px;
  padding-left: 5px;
  border-left: 3px solid @primary-color;
}

.help-text {
  margin-top: 8px;
  font-size: 12px;
  color: #666;
  line-height: 1.5;

  p {
    margin: 4px 0;
  }
}

.cron-link {
  color: @primary-color;
  text-decoration: underline;
  transition: all 0.3s;

  &:hover {
    color: #096dd9;
  }
}

.form-actions {
  margin-top: 30px;
  text-align: center;
}

.ant-form-item {
  margin-bottom: 18px;
}

.ant-radio-button-wrapper-disabled.ant-radio-button-wrapper-checked {
  color: rgb(164 158 158) !important;
  background-color: #e6e6e6 !important;
}
</style>