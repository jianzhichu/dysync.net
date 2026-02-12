<template>
  <a-card :bordered="false" :body-style="{ padding: '10px' }">
    <a-form :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef" label-align="right">
      <!-- 任务调度配置 -->
      <div class="form-section">
        <h3 class="section-title">任务调度</h3>

        <a-form-item has-feedback label="同步周期" name="Cron" :wrapper-col="{ span: 20 }">
          <a-input-number v-model:value="formState.Cron" placeholder="请输入数字" :min="15" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>同步任务执行间隔（分钟），最小15分钟，建议使用默认值</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="单次最多" name="BatchCount" :wrapper-col="{ span: 20 }">
          <a-input-number v-model:value="formState.BatchCount" placeholder="请输入每次下载数量上限(最大40)" :min="10" :max="40" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>每次任务最多下载数量：10-40，拉取视频数据超出该值将会等待下次任务执行。</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="仅新视频" name="OnlySyncNew" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.OnlySyncNew" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，仅同步最近的一页数据（仅对默认收藏夹和点赞视频有效），避免突然大量下载，导致风控</span>
          </div>
        </a-form-item>
      </div>

      <!-- 文件保存配置 -->
      <div class="form-section">
        <h3 class="section-title">博主视频</h3>

        <!-- <a-form-item has-feedback label="默认规则" name="UperUseViedoTitle" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.UperUseViedoTitle" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将使用原标题作为文件名，未开启且未设置标题规则模板，则默认用视频id命名</span>
          </div>
        </a-form-item> -->

        <!-- <a-form-item has-feedback label="统一存储" name="UperSaveTogether" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.UperSaveTogether" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，所有博主视频直接都存一个根目录；关闭后，按博主名称创建文件夹单独存储。</span>
          </div>
        </a-form-item> -->

        <!-- 模板相关配置：只有关闭"标题当文件名"时才显示 -->
        <a-form-item has-feedback label="标题模板" name="FollowedTitleTemplate" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-select v-model:value="formState.FollowedTitleTemplate" :options="template_options" mode="multiple" size="middle" placeholder="请选择占位符组合"></a-select>
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <p></p>
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span style="color: red">
              请选择文件名占位符和模板分隔符（文件名命名规则配置仅博主视频有效，不配置默认使用视频id作为文件名）
            </span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="分隔符" name="FollowedTitleSeparator" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-input v-model:value="formState.FollowedTitleSeparator" placeholder="请输入分隔符（如 - _ 或空）" style="width: 200px" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              占位符之间的连接符（如“-”“_”“.”），为空则直接拼接,不要使用无法用来作为文件名得特殊字符
            </span>
          </div>
        </a-form-item>

        <!-- 完整模板预览：只有关闭"标题当文件名"时才显示 -->
        <a-form-item label="标题预览" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
          <a-input v-model:value="formState.FullFollowedTitleTemplate" placeholder="自定义标题预览" disabled style="background: #f5f5f5" />
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
        <!-- <a-form-item v-if="formState.DownImageVideo" has-feedback label="单独存储" name="ImageViedoSaveAlone" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.ImageViedoSaveAlone" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>
              开启后，图文视频统一存入抖音授权 Cookie 配置的目录，且需提前配置该存储路径。关闭后，则按类型分别存入对应文件夹（如收藏视频存入收藏视频目录）
            </span>
          </div>
        </a-form-item> -->
        <a-form-item has-feedback label="保留音频" name="DownMp3" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownMp3" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将保留音频文件</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="保留图片" name="DownImage" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownImage" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将保留图片文件</span>
          </div>
        </a-form-item>

        <a-form-item has-feedback label="默认音频" name="AudioFile" :wrapper-col="{ span: 20 }">
          <!-- 仅保留检测按钮 -->
          <div class="audio-check-wrapper" style="display: flex; align-items: center; gap: 16px;">
            <a-button type="primary" @click="checkAudioList">
              <play-circle-outlined /> 音频列表
            </a-button>
          </div>

          <!-- 提示文本调整 -->
          <div class="flex items-start mt-1 text-sm text-gray-500" style="color:red">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>当下载图文视频时，音频因版权原因无法下载时，将用该音频文件作为合成视频的音频(仅支持mp3、wav)</span>
          </div>
        </a-form-item>

        <a-modal v-model:visible="audioListModalVisible" title="发现音频" width="666px" destroyOnClose @ok="closeAudioListModal" @cancel="closeAudioListModal">
          <div class="audio-list-container" style="max-height: 500px; overflow-y: auto; padding: 10px 0;">
            <!-- 空列表提示 -->
            <div v-if="audioList.length === 0" style="text-align: center; padding: 40px; color: #999;">
              暂无可用音频文件
            </div>

            <!-- 音频列表（嵌入原生audio播放器） -->
            <a-list v-else bordered :data-source="audioList" item-layout="horizontal">
              <template #renderItem="{item}">
                <a-list-item>
                  <a-list-item-meta :title="item.filename " />
                  <template #actions>
                    <!-- 替换原播放按钮，嵌入原生HTML音频播放器 -->
                    <audio class="native-audio-player" controls :src="'/api/config/getmp3?name='+item.filename" preload="none" @play="handleAudioPlay($event.target)">
                      您的浏览器不支持HTML5音频播放器，请升级浏览器后重试。
                    </audio>
                  </template>
                </a-list-item>
              </template>
            </a-list>
          </div>
        </a-modal>
      </div>

      <div class="form-section">
        <h3 class="section-title">动态视频</h3>

        <a-form-item has-feedback label="下载动态视频" name="DownDynamicVideo" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.DownDynamicVideo" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将下载动态视频（多个短的小视频拼接-合成）</span>
          </div>
        </a-form-item>
        <a-form-item v-if="formState.DownDynamicVideo" has-feedback label="保留原视频" name="KeepDynamicVideo" :wrapper-col="{ span: 20 }">
          <a-switch v-model:checked="formState.KeepDynamicVideo" />
          <div class="flex items-start mt-1 text-sm text-gray-500">
            <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
            <span>开启后，将保留合成视频之前的每个短视频（动态视频会有多个独立的短视频文件，可以选择保留-但不建议，否则emby会出现很多没有封面的视频...）</span>
          </div>
        </a-form-item>
      </div>

      <!-- 系统配置 -->
      <div class="form-section">
        <h3 class="section-title">其他配置</h3>

        <a-form-item v-show="formState.AutoDistinct" has-feedback label="去重优先级" name="PriorityLevel" :wrapper-col="{ span: 20 }">
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
        <a-form-item label="视频编码" name="VideoEncoder">
          <a-radio-group v-model:value="formState.VideoEncoder" button-style="solid">
            <a-radio-button :value="264">H264</a-radio-button>
            <a-radio-button :value="265">H265</a-radio-button>
          </a-radio-group>
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
  <div class="top-right-float-btn-container" v-if="false">
    <a-tooltip title="重置所有刮削" placement="bottom">
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
  //UperSaveTogether: boolean;
  UperUseViedoTitle: boolean;
  LogKeepDay: number;
  DownImage: boolean;
  DownMp3: boolean;
  //ImageViedoSaveAlone: boolean;
  FollowedTitleTemplate: string[];
  FollowedTitleSeparator: string;
  FullFollowedTitleTemplate: string;
  AutoDistinct: boolean;
  PriorityLevel: string;
  DownDynamicVideo: boolean;
  MegDynamicVideo: boolean; // 补充原有缺失字段
  KeepDynamicVideo: boolean; // 补充原有缺失字段
  OnlySyncNew: boolean;
  VideoEncoder: number;
}

// 表单初始数据
const formState: UnwrapRef<FormState> = reactive({
  Cron: 30,
  Id: '0',
  BatchCount: 10,
  LogKeepDay: 10,
  UperUseViedoTitle: false,
  // UperSaveTogether: false,
  DownImageVideo: false,
  DownMp3: false,
  DownImage: false,
  // ImageViedoSaveAlone: true,
  FollowedTitleTemplate: [],
  FollowedTitleSeparator: '',
  FullFollowedTitleTemplate: '',
  AutoDistinct: false,
  PriorityLevel: '',
  DownDynamicVideo: false,
  MegDynamicVideo: false, // 初始化缺失字段
  KeepDynamicVideo: false, // 初始化缺失字段
  OnlySyncNew: false,
  VideoEncoder: 264,
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
          // UperSaveTogether: res.data.uperSaveTogether,
          LogKeepDay: res.data.logKeepDay,
          DownImage: res.data.downImage,
          DownMp3: res.data.downMp3,
          FollowedTitleTemplate: parsedTemplateArr,
          FollowedTitleSeparator: res.data.followedTitleSeparator || '',
          FullFollowedTitleTemplate: fullTemplate,
          // ImageViedoSaveAlone: res.data.imageViedoSaveAlone,
          AutoDistinct: res.data.autoDistinct,
          PriorityLevel: res.data.priorityLevel,
          DownDynamicVideo: res.data.downDynamicVideo,
          MegDynamicVideo: res.data.megDynamicVideo || false, // 补充赋值
          KeepDynamicVideo: res.data.keepDynamicVideo || false, // 补充赋值
          OnlySyncNew: res.data.onlySyncNew,
          VideoEncoder: res.data.videoEncoder,
        });

        tagData.value = JSON.parse(res.data.priorityLevel || '[]');
      } else {
        if (res.message.indexOf('401') == -1) {
          message.error(res.message || '获取配置失败', 8);
        } else {
          console.error('获取配置失败:', res);
        }
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

// 音频列表相关状态
const audioListModalVisible = ref(false); // 弹窗显隐
const audioList = ref<any[]>([]); // 存储接口返回的音频列表
const currentPlayingAudio = ref<HTMLAudioElement | null>(null); // 核心：存储当前正在播放的音频

/**
 * 音频播放互斥处理（核心：同一时间只播放一个音频）
 * @param target 事件目标元素（EventTarget 类型）
 */
const handleAudioPlay = (target: EventTarget | null) => {
  // 1. 校验元素是否存在，且是 HTMLAudioElement 类型
  if (!target || !(target instanceof HTMLAudioElement)) {
    return; // 非音频元素，直接返回，避免报错
  }

  // 2. 此时 target 已被确认是 HTMLAudioElement 类型，可安全操作
  const audioEl = target;
  // 3. 原有互斥逻辑
  if (currentPlayingAudio.value && currentPlayingAudio.value !== audioEl) {
    currentPlayingAudio.value.pause();
  }
  currentPlayingAudio.value = audioEl;
};

/**
 * 检测音频列表：调用mp3List接口并展示弹窗
 */
const checkAudioList = async () => {
  try {
    // 步骤1：打开弹窗前，先暂停上一次可能残留的播放音频
    if (currentPlayingAudio.value) {
      currentPlayingAudio.value.pause();
      currentPlayingAudio.value = null;
    }

    // 步骤2：调用接口获取音频列表
    const res = await useApiStore().mp3List();
    if (res.code === 0) {
      audioList.value = res.data || [];
      audioListModalVisible.value = true;
    } else {
      message.error(res.message || '获取音频列表失败');
    }
  } catch (error) {
    console.error('检测音频列表失败:', error);
    message.error('获取音频列表失败，请稍后重试');
  }
};
/**
 * 关闭音频列表弹窗
 */
const closeAudioListModal = () => {
  audioListModalVisible.value = false;
  // 清理当前播放的音频，避免弹窗关闭后仍在播放或残留实例
  if (currentPlayingAudio.value) {
    currentPlayingAudio.value.pause();
    currentPlayingAudio.value = null;
  }
};
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
// 音频列表弹窗样式优化
:deep(.audio-list-container .ant-list) {
  .ant-list-item {
    padding: 12px 16px;
    &:hover {
      background-color: #fafafa;
    }
  }

  .ant-list-item-meta-title {
    font-weight: 500;
    color: #333;
  }
}

// 检测按钮样式
.audio-check-wrapper {
  padding: 8px 0;
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

// 原生音频播放器样式优化
.native-audio-player {
  width: 200px; // 固定宽度，适配列表布局
  height: 32px; // 统一高度，与列表项对齐
  outline: none; // 移除聚焦轮廓
  border: 1px solid #d9d9d9; // 添边框，与AntD风格统一
  border-radius: 4px; // 圆角，与AntD风格统一
  background-color: #fafafa; // 浅背景色，提升质感

  // 优化播放器内部控件样式（部分浏览器支持）
  &::-webkit-media-controls {
    background-color: #fafafa;
  }

  // 悬浮效果
  &:hover {
    border-color: #1890ff; // 悬浮时边框变主色，提升交互感
    box-shadow: 0 0 0 2px rgba(24, 144, 255, 0.1); // 轻微发光效果
  }
}

// 音频列表项适配，保证播放器与内容对齐
:deep(.ant-list-item-actions) {
  display: flex;
  align-items: center;
  padding-right: 8px;
}
</style>