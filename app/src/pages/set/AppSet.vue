<template>
  <a-card :bordered="false" :body-style="{ padding: '10px' }">
    <a-form :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef" label-align="right">
      <!-- 任务调度配置 -->
      <div class="form-section">
        <h3 class="section-title">任务调度</h3>

        <a-form-item has-feedback label="同步周期（分钟）" name="Cron" :wrapper-col="{ span: 20 }" style="margin-left: 30px">
          <a-input-number v-model:value="formState.Cron" placeholder="请输入数字" :min="15" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>同步任务执行间隔，最小15分钟，建议使用默认值</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="每次同步上限" name="BatchCount" :wrapper-col="{ span: 20 }" style="margin-left: 30px">
          <a-input-number v-model:value="formState.BatchCount" placeholder="请输入每次下载数量上限(最大30)" :min="10" :max="30" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>每次最大下载数量：10-30</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="仅同步最新视频" name="OnlySyncNew" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.OnlySyncNew" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，仅同步最近收藏的20条，以及未来新收藏的，不会去同步之前的视频，默认开启， 避免突然大量下载，导致风控</span>
          </div>
        </a-form-item>
      </div>

      <!-- 文件保存配置 -->
      <div class="form-section">
        <h3 class="section-title">博主视频</h3>

        <a-form-item has-feedback label="标题当文件名" name="UperUseViedoTitle" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.UperUseViedoTitle" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，用原标题作为文件名，不开启但又没设置标题规则模板，则默认用视频id命名</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="不创建子目录" name="UperSaveTogether" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.UperSaveTogether" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>默认按博主名建文件夹，开启后直接存映射目录根目录</span>
          </div>
        </a-form-item>

        <!-- 模板相关配置：只有关闭"标题当文件名"时才显示 -->
        <a-form-item has-feedback label="定义标题模板" name="FollowedTitleTemplate" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-select v-model:value="formState.FollowedTitleTemplate" :options="template_options" mode="multiple" size="middle" placeholder="请选择占位符组合"></a-select>
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <p></p>
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span style="color: red">
              请选择文件名占位符和模板分隔符（文件名命名规则配置仅博主视频有效）
            </span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="模板分隔符" name="FollowedTitleSeparator" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-input v-model:value="formState.FollowedTitleSeparator" placeholder="请输入分隔符（如 - _ 或空）" style="width: 200px" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              占位符之间的连接符（如“-”“_”“.”），为空则直接拼接<br />
              <strong class="text-gray-700">示例：</strong>选择{Author}和{VideoTitle}，分隔符填“-”，将生成“博主名称-视频标题”格式文件名
            </span>
          </div>
        </a-form-item>

        <!-- 完整模板预览：只有关闭"标题当文件名"时才显示 -->
        <a-form-item label="完整模板预览" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-input v-model:value="formState.FullFollowedTitleTemplate" placeholder="最终的模板" disabled style="background: #f5f5f5" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              实时展示您选择的占位符和分隔符组合效果<br />
              <strong class="text-gray-700">示例：</strong>选择{Author}、{ReleaseTime}、{Id}，分隔符填“_”，预览结果为“博主名称_20250101_123456”
            </span>
          </div>
        </a-form-item>
      </div>

      <div class="form-section" v-if="downImgVideo">
        <h3 class="section-title">图文视频</h3>

        <a-form-item has-feedback label="下载图文视频" name="DownImageVideo" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownImageVideo" @change="downImageVideoHandler" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将图片文件和音频文件合成为视频文件</span>
          </div>
        </a-form-item>
        <a-form-item v-if="formState.DownImageVideo" has-feedback label="单独存储" name="ImageViedoSaveAlone" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.ImageViedoSaveAlone" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              开启后，图文视频统一存入抖音授权 Cookie 配置的目录，且需提前配置该存储路径。关闭后，则按类型分别存入对应文件夹（如收藏视频存入收藏视频目录）
            </span>
          </div>
        </a-form-item>
        <a-form-item v-if="formState.DownImageVideo" has-feedback label="保留音频" name="DownMp3" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownMp3" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将保留音频文件</span>
          </div>
        </a-form-item>

        <a-form-item v-if="formState.DownImageVideo" has-feedback label="保留图片" name="DownImage" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownImage" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将保留图片文件</span>
          </div>
        </a-form-item>

        <a-form-item v-if="formState.DownImageVideo" has-feedback label="默认音频" name="AudioFile" :wrapper-col="{ span: 20 }">
          <!-- 音频上传与播放器容器 -->
          <div class="audio-upload-player-wrapper" style="display: flex; align-items: center; gap: 16px;">
            <a-upload :before-upload="beforeUpload" :custom-request="customUpload" :show-upload-list="false" accept=".mp3,.wav">
              <a-button type="default">
                <UploadOutlined /> 选择默认音频文件
              </a-button>
            </a-upload>

            <!-- 新增：启用原生完整控件（controls属性），自带可拖拽进度条 -->
            <div class="audio-player" v-if="audioUrl" style="flex: 1; max-width: 500px;">
              <audio ref="audioInstance" :src="audioUrl" controls controlsList="nodownload" @ended="() => isPlaying = false" @pause="() => isPlaying = false" @play="() => isPlaying = true" class="native-audio-player">
                您的浏览器不支持HTML5音频播放，请升级至现代浏览器。
              </audio>
            </div>
          </div>

          <div class="flex items-start mt-1 text-sm text-gray-500" style="color:red">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>当下载图文视频时，音频因版权原因无法下载时，将用该音频文件作为合成视频的音频</span>
          </div>
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>支持格式：MP3、WAV、AAC、FLAC、OGG、M4A、WMA，单个文件最大20MB</span>
          </div>
        </a-form-item>
      </div>

      <div class="form-section">
        <h3 class="section-title">动态视频</h3>

        <a-form-item has-feedback label="下载动态视频" name="DownDynamicVideo" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownDynamicVideo" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>针对有些视频是多个视频生成的，实际是分为多个视频，开启后将会分别下载多个视频，名字带_001,002这样</span>
          </div>
        </a-form-item>
        <a-form-item v-if="formState.DownDynamicVideo" has-feedback label="合并动态视频" name="MegDynamicVideo" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.MegDynamicVideo" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后将多个动态视频会合并为一个视频</span>
          </div>
        </a-form-item>

        <a-form-item v-if="formState.MegDynamicVideo" has-feedback label="保留原视频" name="KeepDynamicVideo" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.KeepDynamicVideo" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将保留合成视频之前的每个短视频</span>
          </div>
        </a-form-item>
      </div>

      <!-- 系统配置 -->
      <div class="form-section">
        <h3 class="section-title">视频去重</h3>

        <a-form-item v-show="formState.AutoDistinct" has-feedback label="去重优先等级" name="PriorityLevel" :wrapper-col="{ span: 20 }">
          <!-- Tag 拖拽容器 -->
          <div class="tag-drag-container">
            <!-- Tag 拖拽容器（绑定 ref 供 Sortable 初始化） -->
            <div ref="tagContainer" class="tag-list">
              <a-tag v-for="(item, index) in tagData" :key="item.id" class="sortable-tag" color="blue">
                <!-- 用 "≡" 符号替代 a-icon 作为拖拽手柄 -->
                <span class="drag-handle" style="margin-right: 6px; cursor: move;">≡</span>
                {{ item.name }}
              </a-tag>
            </div>
          </div>
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>同一个视频同时存在在多个类型的视频分类时，按照优先级保优先级最高的一个。
              鼠标放到≡，点击鼠标左键即可拖拽调整优先级，</span>
          </div>
        </a-form-item>
      </div>

      <!-- 操作按钮 -->
      <a-form-item :wrapper-col="{ span: 12, offset: 5 }" class="form-actions">
        <a-space size="middle">
          <a-button type="primary" @click="onUpdate" v-if="componentDisabled">
            <SaveOutlined />修改配置
          </a-button>
          <a-button type="primary" danger @click="onSubmit" v-if="!componentDisabled">
            <CheckOutlined />确认保存
          </a-button>
          <a-button type="default" @click="onCancel" v-if="!componentDisabled">取消</a-button>
        </a-space>
      </a-form-item>
    </a-form>
  </a-card>

  <div class="config-float-btn-container">
    <!-- 主按钮 -->
    <a-tooltip title="配置导出导入" placement="left">
      <a-button class="main-float-btn" type="primary" shape="circle">
        <shake-outlined />
      </a-button>
    </a-tooltip>

    <!-- 子按钮容器（新增：绝对定位向上展开） -->
    <div class="float-sub-btn-wrapper">
      <!-- 关键修改：添加 Tooltip 组件包裹导出按钮 -->
      <a-tooltip title="导出配置" placement="left">
        <a-button class="sub-float-btn export-btn" type="default" shape="circle" @click="exportConfig">
          <CloudDownloadOutlined />
        </a-button>
      </a-tooltip>
      <!-- 关键修改：添加 Tooltip 组件包裹导入按钮 -->
      <a-tooltip title="导入配置" placement="left">
        <a-button class="sub-float-btn import-btn" type="default" shape="circle" @click="triggerImportFile">
          <CloudUploadOutlined />
        </a-button>
      </a-tooltip>
    </div>

    <input ref="importFileInput" type="file" accept=".json" class="import-file-input" @change="handleImportFile">
  </div>
  <div class="top-right-float-btn-container">
    <a-tooltip title="重新对同步好的视频文件进行刮削" placement="bottom">
      <a-button class="top-right-float-btn" type="primary" shape="circle" @click="renfo">
        <bulb-outlined />
      </a-button>
    </a-tooltip>
  </div>
</template>

<script lang="ts" setup>
import { reactive, toRaw, ref, watch, onMounted, computed, nextTick } from 'vue';
import type { UnwrapRef } from 'vue';
import { Form, Tooltip } from 'ant-design-vue';
import type { Rule } from 'ant-design-vue/es/form';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { message, Modal } from 'ant-design-vue';
import { Sortable } from 'sortablejs';
import type { UploadProps } from 'ant-design-vue/es/upload/interface';

import {
  InfoCircleOutlined,
  SaveOutlined,
  CheckOutlined,
  ToolOutlined,
  CloudDownloadOutlined,
  CloudUploadOutlined,
  UploadOutlined,
} from '@ant-design/icons-vue';

// 表单引用
const formRef = ref<FormInstance>();

// 模板字段：结构化数组
const template_options = [
  { label: '{Id} (视频ID)', value: '{Id}' },
  { label: '{VideoTitle} (视频标题)', value: '{VideoTitle}' },
  { label: '{ReleaseTime} (发布时间)', value: '{ReleaseTime}' },
  { label: '{Author} (博主名称)', value: '{Author}' },
  { label: '{FileHash} (文件哈希)', value: '{FileHash}' },
  { label: '{Resolution} (分辨率)', value: '{Resolution}' },
];

// 状态定义
const componentDisabled = ref(true);
const downImgVideo = ref(true);

// 新增：悬浮菜单相关状态
const floatMenuVisible = ref(false);
const importFileInput = ref<HTMLInputElement | null>(null);

// 新增：文件上传相关状态（已删除 uploadFileList，仅保留 isUploading 可选）
const isUploading = ref(false);

//开启或关闭合成视频
const downImageVideoHandler = () => {
  if (formState.DownImageVideo) {
  }
};
// 表单数据结构（包含 FullFollowedTitleTemplate 字段）
interface FormState {
  Cron: number;
  Id: string;
  BatchCount: number;
  DownImageVideo: boolean;
  UperSaveTogether: boolean;
  UperUseViedoTitle: boolean;
  LogKeepDay: number;
  DownImage: boolean;
  DownMp3: boolean;
  ImageViedoSaveAlone: boolean;
  FollowedTitleTemplate: string[];
  FollowedTitleSeparator: string;
  FullFollowedTitleTemplate: string;
  AutoDistinct: boolean;
  PriorityLevel: string;
  DownDynamicVideo: boolean;
  MegDynamicVideo: boolean; // 补充原有缺失字段
  KeepDynamicVideo: boolean; // 补充原有缺失字段
  OnlySyncNew: boolean;
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
  DownMp3: false,
  DownImage: false,
  ImageViedoSaveAlone: true,
  FollowedTitleTemplate: [],
  FollowedTitleSeparator: '',
  FullFollowedTitleTemplate: '',
  AutoDistinct: false,
  PriorityLevel: '',
  DownDynamicVideo: false,
  MegDynamicVideo: false, // 初始化缺失字段
  KeepDynamicVideo: false, // 初始化缺失字段
  OnlySyncNew: false,
});

// 实时计算完整模板
const computeFullTemplate = computed(() => {
  return formState.FollowedTitleTemplate.join(formState.FollowedTitleSeparator);
});

// 监听占位符/分隔符变化，实时更新预览
watch(
  [() => [...formState.FollowedTitleTemplate], () => formState.FollowedTitleSeparator],
  () => {
    if (!formState.UperUseViedoTitle) {
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
      formState.FollowedTitleTemplate = [];
      formState.FollowedTitleSeparator = '';
      formState.FullFollowedTitleTemplate = '';
      formRef.value?.clearValidate(['FollowedTitleTemplate', 'FollowedTitleSeparator', 'FullFollowedTitleTemplate']);
    } else {
      formState.FullFollowedTitleTemplate = computeFullTemplate.value;
    }
  },
  { immediate: true }
);

// 表单校验规则
const rules: Record<string, Rule[]> = {
  FollowedTitleSeparator: [{ max: 5, message: '分隔符长度不能超过5个字符', trigger: 'change' }],
  FullFollowedTitleTemplate: [{ max: 200, message: '完整模板字符串长度不能超过200个字符', trigger: 'change' }],
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
        const fullTemplate = res.data.FullFollowedTitleTemplate || res.data.followedTitleTemplate || '';
        const parsedTemplateArr = parseTemplateToArr(fullTemplate);

        Object.assign(formState, {
          Cron: res.data.cron,
          Id: res.data.id,
          BatchCount: res.data.batchCount,
          DownImageVideo: res.data.downImageVideo,
          UperUseViedoTitle: res.data.uperUseViedoTitle,
          UperSaveTogether: res.data.uperSaveTogether,
          LogKeepDay: res.data.logKeepDay,
          DownImage: res.data.downImage,
          DownMp3: res.data.downMp3,
          FollowedTitleTemplate: parsedTemplateArr,
          FollowedTitleSeparator: res.data.followedTitleSeparator || '',
          FullFollowedTitleTemplate: fullTemplate,
          ImageViedoSaveAlone: res.data.imageViedoSaveAlone,
          AutoDistinct: res.data.autoDistinct,
          PriorityLevel: res.data.priorityLevel,
          DownDynamicVideo: res.data.downDynamicVideo,
          MegDynamicVideo: res.data.megDynamicVideo || false, // 补充赋值
          KeepDynamicVideo: res.data.keepDynamicVideo || false, // 补充赋值
          OnlySyncNew: res.data.onlySyncNew,
        });

        tagData.value = JSON.parse(res.data.priorityLevel || '[]');
      } else {
        message.error(res.message || '获取配置失败', 8);
      }
    })
    .catch((error) => {
      console.error('获取配置失败:', error);
      message.error('获取配置失败，请稍后重试', 8);
    });
};

// 模板字符串 → 占位符数组
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

// 拖拽容器 ref
const tagContainer = ref(null);
let sortableInstance = ref<any>(null);
// 模拟 Tag 数据
const tagData = ref([
  { id: 1, name: '喜欢的视频', sort: 1 },
  { id: 2, name: '收藏的视频', sort: 2 },
  { id: 3, name: '关注的视频', sort: 3 },
]);

// 重新计算所有 Tag 的 sort 值
const updateTagSort = () => {
  tagData.value.forEach((item, index) => {
    item.sort = index + 1;
  });
};

// ========== 文件上传相关方法（已修改：移除进度条逻辑） ==========
/**
 * 上传前校验
 */
const beforeUpload: UploadProps['beforeUpload'] = (file) => {
  // 1. 校验文件大小（10MB）
  const isLt20M = file.size / 1024 / 1024 < 20;
  if (!isLt20M) {
    message.error('文件大小不能超过20MB!');
    return false;
  }

  // 2. 校验文件类型
  const acceptTypes = ['.mp3', '.wav', '.aac', '.flac', '.ogg', '.m4a', '.wma'];
  const fileExt = '.' + file.name.split('.').pop()?.toLowerCase();
  if (!acceptTypes.includes(fileExt || '')) {
    message.error('仅支持MP3、WAV、AAC、FLAC、OGG、M4A、WMA格式的音频文件!');
    return false;
  }

  return true;
};
// 1. 新增音频播放相关状态（放在现有状态定义区域，如 isUploading 下方）
const audioUrl = ref('/api/config/defaudio'); // 上传成功后的音频文件URL
const isPlaying = ref(false); // 音频是否正在播放
const audioInstance = ref<HTMLAudioElement | null>(null); // 音频播放器实例

// 2. 改造原有 customUpload 方法，保存上传成功后的音频URL
const customUpload: UploadProps['customRequest'] = (options) => {
  const { file, onSuccess, onError } = options;
  isUploading.value = true;

  // 构造FormData
  const formData = new FormData();
  formData.append('file', file);

  useApiStore()
    .apiUploadAudio(formData)
    .then((res) => {
      console.log(res);
      if (res.code === 0) {
        // message.success('音频文件上传成功!');
        audioUrl.value = `/api/config/defaudio?t=${Date.now()}`;
        onSuccess(res);
      } else {
        message.error(res.message || '文件上传失败!');
        onError(new Error(res.message || '上传失败'), file);
      }
    })
    .catch((error) => {
      console.error('文件上传失败:', error);
      message.error('文件上传失败，请稍后重试!');
      onError(error, file);
    })
    .finally(() => {
      isUploading.value = false;
    });
};

// 新增：封装 load() 为 Promise （可复用）
const audioLoadPromise = (audio: HTMLAudioElement): Promise<void> => {
  return new Promise((resolve, reject) => {
    // 加载成功回调
    const onLoadSuccess = () => {
      audio.removeEventListener('canplaythrough', onLoadSuccess);
      audio.removeEventListener('error', onLoadError);
      resolve();
    };

    // 加载失败回调
    const onLoadError = () => {
      audio.removeEventListener('canplaythrough', onLoadSuccess);
      audio.removeEventListener('error', onLoadError);
      reject(new Error('音频加载失败'));
    };

    audio.addEventListener('canplaythrough', onLoadSuccess);
    audio.addEventListener('error', onLoadError);
    audio.load();
  });
};
// 修改 refreshAudioPlayer 为 async 方法
const refreshAudioPlayer = async () => {
  if (!audioInstance.value) return;

  try {
    // 1. 暂停播放、重置状态
    audioInstance.value.pause();
    audioInstance.value.currentTime = 0;
    isPlaying.value = false;

    // 2. 等待加载完成（核心：解决时序冲突）
    await audioLoadPromise(audioInstance.value);

    // 3. 加载完成后，安全调用 play()
    await audioInstance.value.play();
    isPlaying.value = true;
    // message.success('音频已自动播放');
  } catch (err) {
    if ((err as Error).message !== '音频加载失败') {
      // 区分加载失败和自动播放失败
      message.warning('自动播放失败，请手动点击播放按钮（浏览器限制）');
    } else {
      message.error('音频加载失败，无法自动播放');
    }
    console.log('错误详情:', err);
    isPlaying.value = false;
  }
};
// 监听 audioUrl 变化，重置播放状态（可选，优化用户体验）
watch(
  audioUrl,
  (newVal, oldVal) => {
    // 排除初始值（仅当 url 发生有效变更时刷新）
    if (newVal && newVal !== oldVal) {
      isPlaying.value = false;
      // 核心：调用刷新方法，强制加载新音频
      refreshAudioPlayer();
    } else if (!newVal) {
      // 若 url 为空，仅重置状态
      isPlaying.value = false;
      if (audioInstance.value) {
        audioInstance.value.currentTime = 0;
      }
    }
  },
  { immediate: false }
); // 关闭 immediate，避免初始加载时触发

// 组件挂载时获取配置
onMounted(async () => {
  getConfig();
  await nextTick();
  if (tagContainer.value) {
    sortableInstance.value = new Sortable(tagContainer.value, {
      handle: '.drag-handle',
      animation: 150,
      ghostClass: 'tag-ghost',
      preventOnFilter: true,
      onEnd: (evt) => {
        const [movedTag] = tagData.value.splice(evt.oldIndex, 1);
        tagData.value.splice(evt.newIndex, 0, movedTag);
        updateTagSort();
        console.log('最新 Tag 数据（含 sort）：', tagData.value);
      },
    });
  } else {
    console.warn('拖拽容器未找到，请检查 ref 绑定');
  }
});

// 提交表单
const onSubmit = () => {
  formRef.value
    ?.validate()
    .then(() => {
      let fullTemplate = '';
      if (!formState.UperUseViedoTitle) {
        fullTemplate = formState.FollowedTitleTemplate.join(formState.FollowedTitleSeparator);
      }

      const submitData = {
        ...toRaw(formState),
        FullFollowedTitleTemplate: fullTemplate,
        FollowedTitleTemplate: formState.FollowedTitleTemplate.join(''),
        PriorityLevel: JSON.stringify(tagData.value),
      };

      useApiStore()
        .apiUpdateConfig(submitData)
        .then((res) => {
          if (res.code === 0) {
            message.success('修改成功，同步任务将在5-10秒按新配置运行...');
            componentDisabled.value = true;
          } else {
            message.error(res.message || '更新配置失败', 8);
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
  if (!formState.UperUseViedoTitle) {
    formState.FullFollowedTitleTemplate = computeFullTemplate.value;
  }
};

const importOrexportConf = ref<any>(null);
// 导出配置
const exportConfig = () => {
  try {
    useApiStore()
      .ExportConf()
      .then((res) => {
        if (res.code === 0) {
          importOrexportConf.value = res.data;

          const jsonStr = JSON.stringify(importOrexportConf.value, null, 2);
          const blob = new Blob([jsonStr], { type: 'application/json' });

          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `配置及手动关注列表备份_${new Date().getTime()}.json`;
          document.body.appendChild(a);
          a.click();

          document.body.removeChild(a);
          URL.revokeObjectURL(url);

          message.success('导出配置及手动关注列表导出成功');
          floatMenuVisible.value = false;
        }
      })
      .catch((x) => {
        console.error('导出接口请求失败:', x);
      });
  } catch (error) {
    console.error('导出配置及手动关注列表失败:', error);
    message.error('导出配置及手动关注列表失败，请稍后重试');
  }
};

// 触发导入文件选择
const triggerImportFile = () => {
  if (importFileInput.value) {
    importFileInput.value.click();
  }
};

// 处理导入文件
const handleImportFile = (e: Event) => {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];

  if (!file) {
    return;
  }

  if (file.type !== 'application/json' && !file.name.endsWith('.json')) {
    message.error('请选择JSON格式的配置文件');
    target.value = '';
    return;
  }

  const reader = new FileReader();
  reader.onload = (event) => {
    try {
      const content = event.target?.result as string;
      const importData = JSON.parse(content);
      useApiStore()
        .ImportConf(importData)
        .then((res) => {
          if (res.code === 0) {
            message.success('配置及手动关注列表导入成功,下次运行会按新的配置规则运行。');
            floatMenuVisible.value = false;
            getConfig();
          } else {
            message.error('配置及手动关注列表导入失败');
          }
        });
    } catch (error) {
      console.error('解析配置文件失败:', error);
      message.error(`配置及手动关注列表导入失败：${(error as Error).message}`);
    } finally {
      target.value = '';
    }
  };

  reader.readAsText(file);
};
const renfo = () => {
  // AntD 确认弹窗：Modal.confirm
  Modal.confirm({
    title: '操作确认', // 弹窗标题
    content:
      '确认要重新生成所有视频的.nfo刮削文件吗？根据视频数量，该操作可能需要较长时间，操作后可查看日志信息，然后进emby查看最新刮削信息。', // 弹窗提示内容
    okText: '确认执行', // 确认按钮文本
    cancelText: '取消', // 取消按钮文本
    iconType: 'warning', // 警告图标（强化提醒效果，防误操作）
    // 用户点击「确认」时触发
    onOk() {
      // 执行原有接口请求逻辑
      useApiStore()
        .Renfo()
        .then((r) => {
          if (r.code == 0) {
            message.success(
              '所有视频将重新生成.nfo刮削文件。根据视频数量，可能需要的时间不同，稍后可以到emby查看最新的刮削信息'
            );
          }
        });
    },
    onCancel() {},
  });
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

// 模板选择器选项样式优化
:deep(.ant-select-item-option-content) {
  display: flex;
  align-items: center;
}

// 提示信息中的强调文本样式
:deep(.text-gray-700) {
  color: #444 !important;
  font-weight: 500;
}

/* 拖拽容器样式 */
.tag-drag-container {
  position: relative;
  overflow: visible !important;
}

/* Tag 列表布局（换行显示） */
.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

/* 拖拽中的 Tag 占位样式 */
.tag-ghost {
  opacity: 0.5;
  background-color: #e6f7ff !important;
  border-color: #91d5ff !important;
}

/* 拖拽手柄样式优化 */
.drag-handle {
  font-size: 14px;
  color: #666;
  user-select: none;
}

.drag-handle:hover {
  color: #1890ff;
}

/* Tag  hover 效果 */
.sortable-tag {
  transition: all 0.2s;
}

.sortable-tag:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

// 悬浮按钮容器
.config-float-btn-container {
  position: fixed;
  right: 30px;
  bottom: 100px;
  z-index: 1000;
  width: 60px;
  height: auto;

  &:hover {
    cursor: default;
  }
}

// 子按钮容器
.float-sub-btn-wrapper {
  position: absolute;
  bottom: 70px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

// 主按钮样式
.main-float-btn {
  width: 60px;
  height: 60px;
  font-size: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    transform: scale(1.05);
  }
}

// 子按钮样式
.sub-float-btn {
  width: 50px;
  height: 50px;
  font-size: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  opacity: 0;
  transform: translateY(10px) scale(0.9);
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);

  &.export-btn {
    transition-delay: 0.08s;
  }

  &.import-btn {
    transition-delay: 0.16s;
  }
}

.config-float-btn-container:hover .sub-float-btn {
  opacity: 1;
  transform: translateY(0) scale(1);
}

// 隐藏文件导入输入框
.import-file-input {
  display: none;
}

// 修复子按钮tooltip层级
:deep(.ant-tooltip) {
  z-index: 1001 !important;
}

// 上传组件样式适配
:deep(.ant-upload) {
  display: inline-block;
}

:deep(.ant-upload.ant-upload-select) {
  display: inline-block;
}

// 音频上传与播放器容器样式
.audio-upload-player-wrapper {
  flex-wrap: wrap;
  @media (max-width: 768px) {
    flex-direction: column;
    align-items: flex-start;
  }
}

// 音频播放器样式优化
:deep(.audio-player audio) {
  // border: 1px solid #d9d9d9;
  border-radius: 4px;
  padding: 2px;
  &:hover {
    border-color: #1890ff;
  }
}

// 播放/清空按钮 hover 效果
:deep(.audio-player .ant-btn-text:hover) {
  background-color: #f5f5f5;
}

// 右上角悬浮按钮容器
.top-right-float-btn-container {
  position: fixed;
  top: 100px; // 距离顶部间距
  right: 30px; // 距离右侧间距（与现有底部按钮对齐）
  z-index: 1000; // 保证悬浮在最上层
}

// 右上角悬浮按钮样式
.top-right-float-btn {
  width: 56px;
  height: 56px;
  font-size: 18px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  opacity: 0.6; // 默认半透明（0-1之间，0.6为适中的半透明效果）
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); // 平滑过渡动画

  // 鼠标悬浮时：移除半透明，轻微放大增强交互感
  &:hover {
    opacity: 1; // 移除半透明，完全不透明
    transform: scale(1.05); // 轻微放大，提升交互体验（可选，可删除）
    box-shadow: 0 6px 16px rgba(0, 0, 0, 0.15); // 悬浮时阴影加深（可选，可删除）
  }
}
</style>