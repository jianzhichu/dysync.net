<template>
  <a-card :bordered="false" :body-style="{ padding: '20px' }">
    <a-form :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef" label-align="right">
      <!-- 任务调度配置 -->
      <div class="form-section">
        <h3 class="section-title">任务调度</h3>

        <a-form-item has-feedback label="同步周期（分钟）" name="Cron" :wrapper-col="{ span: 6}" style="margin-left:30px">
          <a-input-number v-model:value="formState.Cron" placeholder="请输入数字" :min="15" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>同步任务的执行间隔，最小15分钟，建议使用默认值</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="每次同步（条数）" name="BatchCount" :wrapper-col="{ span:6}" style="margin-left:30px">
          <a-input-number v-model:value="formState.BatchCount" placeholder="请输入查询条数(最大30)" :min="10" :max="30" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>每次同步获取的条数，范围10-30条，建议使用默认值</span>
          </div>
        </a-form-item>
      </div>

      <!-- 文件保存配置 -->
      <div class="form-section">
        <h3 class="section-title">博主视频（仅关注有效）</h3>

        <a-form-item has-feedback label="标题当文件名" name="UperUseViedoTitle" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.UperUseViedoTitle" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>启用后，关注的视频文件名将直接使用原视频标题，否则使用模板生成,如果既没有模板也没有开启，则系统默认使用视频Id作为文件名。</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="不创建子目录" name="UperSaveTogether" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.UperSaveTogether" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>默认是按博主名字创建文件夹存储，启用后，关注的视频直接存放在根目录。</span>
          </div>
        </a-form-item>

        <!-- 模板相关配置：只有关闭"标题当文件名"时才显示 -->
        <a-form-item has-feedback label="定义标题模板" name="FollowedTitleTemplate" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-select v-model:value="formState.FollowedTitleTemplate" :options="template_options" mode="multiple" size="middle" placeholder="请选择"></a-select>
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>选择生成文件名的占位符，顺序即为文件名顺序（需配合分隔符使用）</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="模板分隔符" name="FollowedTitleSeparator" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-input v-model:value="formState.FollowedTitleSeparator" placeholder="请输入分隔符（如 - _ 或空）" style="width: 200px" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>占位符之间的连接符（如“-”“_”），为空则直接拼接</span>
          </div>
        </a-form-item>

        <!-- 完整模板预览：只有关闭"标题当文件名"时才显示 -->
        <a-form-item label="完整模板预览" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-input v-model:value="formState.FullFollowedTitleTemplate" placeholder="最终的模板" disabled style="background: #f5f5f5" />
        </a-form-item>
      </div>

      <div class="form-section" v-if="downImgVideo">
        <h3 class="section-title">图文视频</h3>

        <a-form-item has-feedback label="图文视频合成" name="DownImageVideo" :wrapper-col="{ span: 6 }">
          <a-switch v-model:checked="formState.DownImageVideo" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>启用后，图文视频将合成为视频文件下载</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="额外下载音频" name="DownMp3" :wrapper-col="{ span: 6 }">
          <a-switch v-model:checked="formState.DownMp3" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>启用后，将额外下载音频文件</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="额外下载图片" name="DownImage" :wrapper-col="{ span: 6 }">
          <a-switch v-model:checked="formState.DownImage" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>启用后，将额外下载所有图片文件</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="是否统一存储" name="ImageViedoSaveAlone" :wrapper-col="{ span: 10 }">
          <a-switch v-model:checked="formState.ImageViedoSaveAlone" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              启用后，所有图文视频统一存储到 Cookie(抖音授权) 设置的目录。<br />否则，按类型存储到对应文件夹（比如视频属于收藏的视频，则存储到收藏视频的目录）
            </span>
          </div>
        </a-form-item>
      </div>

      <!-- 系统配置 -->
      <div class="form-section">
        <h3 class="section-title">其他配置</h3>

        <a-form-item has-feedback label="日志保留（天数）" name="LogKeepDay" :wrapper-col="{ span: 6 }" style="margin-left:30px">
          <a-input-number v-model:value="formState.LogKeepDay" placeholder="请输入保留天数" :min="1" :max="90" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>系统运行日志的保留天数，范围1-90天，过期自动清理</span>
          </div>
        </a-form-item>
        <a-form-item has-feedback label="是否统一存储" name="AutoDistinct" :wrapper-col="{ span: 10 }">
          <a-switch v-model:checked="formState.AutoDistinct" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              启用后，同一个视频,只会下载一次。
            </span>
          </div>
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
import { reactive, toRaw, ref, watch, onMounted, computed } from 'vue';
import type { UnwrapRef } from 'vue';
import { Form } from 'ant-design-vue';
import type { Rule } from 'ant-design-vue/es/form';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { message } from 'ant-design-vue';
import { InfoCircleOutlined } from '@ant-design/icons-vue';

// 表单引用
const formRef = ref<FormInstance>();

// 模板字段：结构化数组
const template_options = [
  { label: '{Id}', value: '{Id}' },
  { label: '{VideoTitle}', value: '{VideoTitle}' },
  { label: '{FileHash}', value: '{FileHash}' },
  // { label: '{FileSize}', value: '{FileSize}' },
  { label: '{Resolution}', value: '{Resolution}' },
  // { label: '{SyncTime}', value: '{SyncTime}' },
  { label: '{ReleaseTime}', value: '{ReleaseTime}' },
];

// 状态定义
const componentDisabled = ref(true);
const downImgVideo = ref(false);

// 表单数据结构（新增 FullFollowedTitleTemplate 字段）
interface FormState {
  Cron: number;
  Id: string;
  BatchCount: number;
  DownImageVideoFromEnv: boolean;
  DownImageVideo: boolean;
  UperSaveTogether: boolean;
  UperUseViedoTitle: boolean;
  LogKeepDay: number;
  DownImage: boolean;
  DownMp3: boolean;
  ImageViedoSaveAlone: boolean;
  FollowedTitleTemplate: string[]; // 占位符数组
  FollowedTitleSeparator: string; // 分隔符
  FullFollowedTitleTemplate: string; // 新增：完整模板字符串（自动生成）
  AutoDistinct: boolean;
}

// 表单初始数据
const formState: UnwrapRef<FormState> = reactive({
  Cron: 30,
  Id: '0',
  BatchCount: 10,
  LogKeepDay: 10,
  UperUseViedoTitle: false,
  UperSaveTogether: false,
  DownImageVideo: false,
  DownImageVideoFromEnv: false,
  DownMp3: false,
  DownImage: false,
  ImageViedoSaveAlone: true,
  FollowedTitleTemplate: [],
  FollowedTitleSeparator: '',
  FullFollowedTitleTemplate: '', // 初始为空
  AutoDistinct: false,
});

// 实时计算完整模板（可选：让用户实时预览，提交时无需重复计算）
const computeFullTemplate = computed(() => {
  return formState.FollowedTitleTemplate.join(formState.FollowedTitleSeparator);
});

// 监听占位符/分隔符变化，实时更新预览（可选）
watch(
  [() => [...formState.FollowedTitleTemplate], () => formState.FollowedTitleSeparator],
  () => {
    if (!formState.UperUseViedoTitle) {
      // 只有关闭标题当文件名时才更新预览
      formState.FullFollowedTitleTemplate = computeFullTemplate.value;
    }
  },
  { immediate: true, deep: true }
);

// 监听"标题当文件名"开关变化
watch(
  () => formState.UperUseViedoTitle,
  (isEnabled) => {
    if (isEnabled) {
      // 开启时清空模板相关数据，避免数据残留
      formState.FollowedTitleTemplate = [];
      formState.FollowedTitleSeparator = '';
      formState.FullFollowedTitleTemplate = '';
      // 清空相关表单项的校验状态
      formRef.value?.clearValidate(['FollowedTitleTemplate', 'FollowedTitleSeparator', 'FullFollowedTitleTemplate']);
    } else {
      // 关闭时重新计算完整模板
      formState.FullFollowedTitleTemplate = computeFullTemplate.value;
    }
  },
  { immediate: true }
);

// 表单校验规则（新增 FullFollowedTitleTemplate 校验）
const rules: Record<string, Rule[]> = {
  FollowedTitleSeparator: [{ max: 5, message: '分隔符长度不能超过5个字符', trigger: 'change' }],
  FullFollowedTitleTemplate: [{ max: 200, message: '完整模板字符串长度不能超过200个字符', trigger: 'change' }],
};

// 布局配置
const labelCol = { style: { width: '150px', textAlign: 'right' } };
const wrapperCol = { span: 12 };

// 获取配置数据（适配 FullFollowedTitleTemplate 字段）
const getConfig = () => {
  useApiStore()
    .apiGetConfig()
    .then((res) => {
      if (res.code === 0) {
        // 优先从 FullFollowedTitleTemplate 解析占位符数组（确保数据一致）
        const fullTemplate = res.data.FullFollowedTitleTemplate || res.data.followedTitleTemplate || '';
        const parsedTemplateArr = parseTemplateToArr(fullTemplate);

        // 赋值所有字段（包含新增的 FullFollowedTitleTemplate）
        Object.assign(formState, {
          Cron: res.data.cron,
          Id: res.data.id,
          BatchCount: res.data.batchCount,
          DownImageVideoFromEnv: res.data.downImageVideoFromEnv,
          DownImageVideo: res.data.downImageVideo,
          UperUseViedoTitle: res.data.uperUseViedoTitle,
          UperSaveTogether: res.data.uperSaveTogether,
          LogKeepDay: res.data.logKeepDay,
          DownImage: res.data.downImage,
          DownMp3: res.data.downMp3,
          FollowedTitleTemplate: parsedTemplateArr,
          FollowedTitleSeparator: res.data.followedTitleSeparator || '',
          FullFollowedTitleTemplate: fullTemplate, // 回显完整模板
          ImageViedoSaveAlone: res.data.imageViedoSaveAlone,
          AutoDistinct: res.data.autoDistinct,
        });

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

// 模板字符串 → 占位符数组（原有逻辑不变）
const parseTemplateToArr = (templateStr: string | null | undefined) => {
  if (!templateStr || typeof templateStr !== 'string' || templateStr.trim() === '') {
    return [];
  }
  const placeholderReg = /\{[a-zA-Z0-9]+\}/g;
  const matchedPlaceholders = templateStr.match(placeholderReg) || [];
  const validPlaceholderValues = template_options.map((item) => item.value);
  const validPlaceholders = matchedPlaceholders.filter((placeholder) => validPlaceholderValues.includes(placeholder));
  return Array.from(new Set(validPlaceholders));
};

// 组件挂载时获取配置
onMounted(() => {
  getConfig();
});

// 提交表单（核心：自动生成完整模板字符串）
const onSubmit = () => {
  formRef.value
    .validate()
    .then(() => {
      // 1. 如果开启了标题当文件名，清空模板相关字段
      let fullTemplate = '';
      let templateTitle = '';
      if (!formState.UperUseViedoTitle) {
        // 自动拼接完整模板字符串（数组 + 分隔符）
        fullTemplate = formState.FollowedTitleTemplate.join(formState.FollowedTitleSeparator);
      }

      // 2. 构造提交数据（包含三个模板相关字段）
      const submitData = {
        ...toRaw(formState),
        FullFollowedTitleTemplate: fullTemplate, // 确保提交最新拼接结果
        FollowedTitleTemplate: formState.FollowedTitleTemplate.join(''),
      };

      // 3. 提交接口
      useApiStore()
        .apiUpdateConfig(submitData)
        .then((res) => {
          if (res.code === 0) {
            message.success('修改成功，同步任务将在5-10秒按新配置运行...');
            componentDisabled.value = true;
          } else {
            message.error(res.erro || '更新配置失败', 8);
          }
        })
        .catch((error) => {
          console.error('更新配置失败:', error);
          message.error('修改失败，请稍后重试', 8);
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
  formRef.value?.clearValidate();
  // 取消时恢复完整模板预览（只有关闭标题当文件名时）
  if (!formState.UperUseViedoTitle) {
    formState.FullFollowedTitleTemplate = computeFullTemplate.value;
  }
};
</script>

<style lang='less' scoped>
@primary-color: #1890ff;
@text-color: #333;
@border-radius: 4px;

.form-section {
  &:last-child {
    border-bottom: none;
  }
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #f0f0f0;
}

.section-title {
  font-size: 16px;
  font-weight: 500;
  color: @primary-color;
  margin-bottom: 15px;
  padding-left: 5px;
  border-left: 3px solid @primary-color;
}

.ant-form-item {
  margin-bottom: 18px;
}

.ant-radio-button-wrapper-disabled.ant-radio-button-wrapper-checked {
  color: rgb(164 158 158) !important;
  background-color: #e6e6e6 !important;
}

.ant-form-item-label {
  text-align: left !important;
}

// 统一提醒文字样式
:deep(.flex.items-start.mt-1.text-sm.text-gray-500) {
  line-height: 1.6;
  white-space: normal;
}

// 禁用输入框样式优化
:deep(.ant-input-disabled) {
  color: #666 !important;
}
</style>