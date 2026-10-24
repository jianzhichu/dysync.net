<template>
  <a-card class="system-config-page" :bordered="false" :body-style="{ padding: '10px' }">
    <a-form class="system-config-form" :model="formState" :label-col="labelCol" :rules="rules" :wrapper-col="wrapperCol" ref="formRef" label-align="right">
      <a-tabs class="settings-tabs" default-active-key="sync">
        <a-tab-pane key="sync" tab="同步配置">
          <div class="settings-tab-panel settings-tab-panel--narrow">
            <div class="form-section">
              <h3 class="section-title">任务调度</h3>
              <div class="sync-settings-grid">
                <a-form-item has-feedback label="同步周期" name="Cron" :wrapper-col="{ span: 20 }">
                  <a-input v-model:value="formState.Cron" class="sync-number-input" inputmode="numeric" placeholder="请输入数字" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>间隔单位为分钟，最短 15 分钟。</span>
                  </div>
                </a-form-item>

                <a-form-item has-feedback label="单次最多" name="BatchCount" :wrapper-col="{ span: 20 }">
                  <a-input v-model:value="formState.BatchCount" class="sync-number-input" inputmode="numeric" placeholder="请输入每次下载数量上限(最大40)" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>每次下载 10–40 条；超出部分将在下次任务执行时继续下载。</span>
                  </div>
                </a-form-item>

                <a-form-item class="sync-only-new" has-feedback label="仅新视频" name="OnlySyncNew" :wrapper-col="{ span: 20 }">
                  <a-switch v-model:checked="formState.OnlySyncNew" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>仅同步最近一页（限点赞和默认收藏），减少集中下载。</span>
                  </div>
                </a-form-item>
                <a-form-item has-feedback label="Debug 日志" name="DebugLogEnabled" :wrapper-col="{ span: 20 }">
                  <a-switch v-model:checked="formState.DebugLogEnabled" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>开启后记录 Debug 级别日志；关闭后重启服务生效。</span>
                  </div>
                </a-form-item>
              </div>
            </div>
          </div>
        </a-tab-pane>
        <a-tab-pane key="download" tab="下载策略">
          <div class="settings-tab-panel settings-tab-panel--download">
            <div class="form-section media-download-settings">
              <h3 class="section-title">&#35270;&#39057;&#19979;&#36733;</h3>
              <div class="media-switch-grid">
                <a-form-item has-feedback label="动态视频" name="DownDynamicVideo" :wrapper-col="{ span: 20 }">
                  <a-switch v-model:checked="formState.DownDynamicVideo" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>下载动态片段并合成为一个视频。</span>
                  </div>
                </a-form-item>
                <a-form-item has-feedback label="保留原视频" name="KeepDynamicVideo" :wrapper-col="{ span: 20 }">
                  <a-switch v-model:checked="formState.KeepDynamicVideo" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>保留合成前的片段文件，前提要开启动态视频下载。</span>
                  </div>
                </a-form-item>
              </div>
              <div v-if="downImgVideo">

                <a-form-item has-feedback label="图文视频" name="DownImageVideo" :wrapper-col="{ span: 20 }">
                  <a-switch v-model:checked="formState.DownImageVideo" @change="downImageVideoHandler" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>将图文视频中的图片和音频合成为视频。</span>
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
                <div class="media-switch-grid">
                  <a-form-item has-feedback label="保留音频" name="DownMp3" :wrapper-col="{ span: 20 }">
                    <a-switch v-model:checked="formState.DownMp3" />
                    <div class="flex items-start mt-1 text-sm text-gray-500">
                      <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                      <span>额外保留原音频文件。</span>
                    </div>
                  </a-form-item>

                  <a-form-item has-feedback label="保留图片" name="DownImage" :wrapper-col="{ span: 20 }">
                    <a-switch v-model:checked="formState.DownImage" />
                    <div class="flex items-start mt-1 text-sm text-gray-500">
                      <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                      <span>额外保留原图片文件。</span>
                    </div>
                  </a-form-item>
                </div>

                <a-form-item has-feedback label="默认音频" name="AudioFile" :wrapper-col="{ span: 20 }">
                  <!-- 仅保留检测按钮 -->
                  <div class="audio-check-wrapper" style="display: flex; align-items: center; gap: 16px;">
                    <a-button class="audio-list-button" type="primary" size="small" aria-label="选择默认音频" title="选择默认音频" @click="checkAudioList">
                      <play-circle-outlined />
                    </a-button>
                  </div>

                  <!-- 提示文本调整 -->
                  <div class="flex items-start mt-1 text-sm text-gray-500" style="color:red">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>图文音频无法下载时用此文件替代；仅支持 MP3、WAV。</span>
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
            </div>
            <div class="form-section download-preferences">
              <h3 class="section-title">下载偏好</h3>
              <div class="download-preference-grid">
                <a-form-item class="preference-priority" v-show="formState.AutoDistinct" has-feedback label="去重优先级" name="PriorityLevel" :wrapper-col="{ span: 20 }">
                  <div class="tag-drag-container">
                    <div class="tag-list">
                      <div v-for="(item, index) in tagData" :key="item.id" class="priority-row">
                        <div class="priority-label">
                          <span class="priority-index">{{ index + 1 }}</span>
                          <span>{{ item.name }}</span>
                        </div>
                        <div class="priority-controls">
                          <a-button class="priority-move-button" type="text" size="small" :disabled="index === 0" :aria-label="`左移${item.name}`" title="左移" @click.stop="movePriority(index, -1)">&#8592;</a-button>
                          <a-button class="priority-move-button" type="text" size="small" :disabled="index === tagData.length - 1" :aria-label="`右移${item.name}`" title="右移" @click.stop="movePriority(index, 1)">&#8594;</a-button>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>同一视频仅保留优先级最高的一份；点击箭头调整顺序。</span>
                  </div>
                </a-form-item>
                <a-form-item label="视频画质" name="VideoQuality">
                  <a-radio-group v-model:value="formState.VideoQuality" button-style="solid" size="small">
                    <a-radio-button :value="0">原画 (302直链/极清)</a-radio-button>
                    <a-radio-button :value="1080">1080P 超清</a-radio-button>
                    <a-radio-button :value="720">720P 高清</a-radio-button>
                  </a-radio-group>
                </a-form-item>
                <a-form-item label="视频编码" name="VideoEncoder">
                  <a-radio-group v-model:value="formState.VideoEncoder" button-style="solid" size="small">
                    <a-radio-button :value="265">H265 (优先)</a-radio-button>
                    <a-radio-button :value="264">H264 (兼容)</a-radio-button>
                  </a-radio-group>
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span v-if="formState.VideoEncoder === 265">优先下载 H.265；没有可用 H.265 码流时回退到 H.264。</span><span v-else>优先下载 H.264；没有可用 H.264 码流时回退到 H.265。</span>
                  </div>
                </a-form-item>
                <a-form-item label="关闭刮削" name="CloseNfo">
                  <a-switch v-model:checked="formState.CloseNfo" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>关闭后不生成 NFO，也不下载视频封面和作者头像，Emby、Jellyfin 等媒体库可能无法完整刮削和展示视频信息。</span>
                  </div>
                </a-form-item>
                <a-form-item label="检查文件" name="SkipFileExistenceCheck">
                  <a-switch v-model:checked="checkFileExistence" />
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>开启：检查数据库记录对应的本地文件；文件已移动或删除时会重新下载。关闭：只要数据库有记录就跳过，不检查文件是否还在，可能导致视频无法播放。</span>
                  </div>
                </a-form-item>
              </div>
            </div>
          </div>
        </a-tab-pane>
        <a-tab-pane key="naming" tab="命名规则">
          <div class="settings-tab-panel settings-tab-panel--narrow">
            <div class="form-section filename-settings">
              <h3 class="section-title">视频文件命名规则</h3>

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
              <div class="filename-settings-grid">
                <a-form-item class="title-template-item" has-feedback label="标题模板" name="FollowedTitleTemplate" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
                  <a-select v-model:value="formState.FollowedTitleTemplate" :options="template_options" mode="multiple" size="middle" placeholder="请选择占位符组合"></a-select>
                  <div class="flex items-start mt-1 text-sm text-gray-500">
                    <p></p>
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span style="color: red">适用于点赞、收藏、关注和自定义收藏夹；合集、短剧仍按集数命名。留空时使用视频 ID。</span>
                  </div>
                </a-form-item>

                <div class="filename-preview-grid">
                  <a-form-item class="title-separator-item" has-feedback label="分隔字符" name="FollowedTitleSeparator" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
                    <a-input v-model:value="formState.FollowedTitleSeparator" placeholder="请输入分隔符（如 - _ 或空）" style="width: 200px" />
                    <div class="flex items-start mt-1 text-sm text-gray-500">
                      <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                      <span>占位符之间的连接符，可留空；请勿使用文件名禁用字符。</span>
                    </div>
                  </a-form-item>

                  <!-- 完整模板预览：只有关闭"标题当文件名"时才显示 -->
                  <a-form-item class="title-preview-item" label="标题预览" :wrapper-col="{ span: 12 }" v-if="!formState.UperUseViedoTitle">
                    <a-input v-model:value="formState.FullFollowedTitleTemplate" placeholder="自定义标题预览" disabled style="background: #f5f5f5" />
                  </a-form-item>
                </div>
              </div>
            </div>
          </div>
        </a-tab-pane>
        <a-tab-pane key="storage" tab="存储位置">
          <div class="settings-tab-panel">
            <div class="form-section storage-config-section">
              <h3 class="section-title">扩展存储</h3>
              <div class="storage-root-settings-grid">
                <div class="storage-setting">
                  <label class="storage-setting-label" for="storage-expansion-roots">扩展存储根目录</label>
                  <a-select v-if="isDockerDeployment" id="storage-expansion-roots" v-model:value="selectedDockerExpansionRoots" mode="multiple" :loading="storageRootsLoading" :max-tag-count="'responsive'" placeholder="请选择已在 Docker Compose 中挂载的扩展目录" style="width: 100%">
                    <a-select-option v-for="path in dockerExpansionRoots" :key="path" :value="path">{{ path }}</a-select-option>
                  </a-select>
                  <a-input v-else id="storage-expansion-roots" v-model:value="formState.StorageExpansionRootsText" :disabled="componentDisabled" placeholder="多个根目录用英文分号 ; 隔开，例如 D:\\DouyinStorage1; D:\\DouyinStorage2" />
                  <div class="storage-setting-help">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span v-if="isDockerDeployment">仅显示已挂载且容器目录名以 dyapp_ 开头的路径；新增挂载后需重建容器。</span>
                    <span v-else>多个路径用英文分号分隔；程序会自动创建子目录。</span>
                  </div>
                </div>
                <div class="storage-setting">
                  <label class="storage-setting-label" for="storage-min-free-space">切换阈值（GB）</label>
                  <a-input-number id="storage-min-free-space" v-model:value="formState.StorageMinFreeSpaceGb" :min="0" :max="100000" />
                  <div class="storage-setting-help">
                    <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                    <span>剩余空间低于此值时，自动切换到其他可用根目录。</span>
                  </div>
                </div>
              </div>
              <div class="storage-active-roots-grid">
                <div class="storage-setting">
                  <label class="storage-setting-label">收藏视频</label>
                  <a-select v-model:value="formState.ActiveCollectStorageRootIndex" style="width: 100%">
                    <a-select-option :value="0">原收藏路径</a-select-option>
                    <a-select-option v-for="(path, index) in expansionRoots" :key="`collect-${path}`" :value="index + 1">{{ path }}</a-select-option>
                  </a-select>
                </div>
                <div class="storage-setting">
                  <label class="storage-setting-label">喜欢视频</label>
                  <a-select v-model:value="formState.ActiveFavoriteStorageRootIndex" style="width: 100%">
                    <a-select-option :value="0">原喜欢路径</a-select-option>
                    <a-select-option v-for="(path, index) in expansionRoots" :key="`favorite-${path}`" :value="index + 1">{{ path }}</a-select-option>
                  </a-select>
                </div>
                <div class="storage-setting">
                  <label class="storage-setting-label">博主视频</label>
                  <a-select v-model:value="formState.ActiveFollowedStorageRootIndex" style="width: 100%">
                    <a-select-option :value="0">原博主路径</a-select-option>
                    <a-select-option v-for="(path, index) in expansionRoots" :key="`followed-${path}`" :value="index + 1">{{ path }}</a-select-option>
                  </a-select>
                </div>
                <div class="storage-setting">
                  <label class="storage-setting-label">合集视频</label>
                  <a-select v-model:value="formState.ActiveMixStorageRootIndex" style="width: 100%">
                    <a-select-option :value="0">原合集路径</a-select-option>
                    <a-select-option v-for="(path, index) in expansionRoots" :key="`mix-${path}`" :value="index + 1">{{ path }}</a-select-option>
                  </a-select>
                </div>
                <div class="storage-setting">
                  <label class="storage-setting-label">短剧视频</label>
                  <a-select v-model:value="formState.ActiveSeriesStorageRootIndex" style="width: 100%">
                    <a-select-option :value="0">原短剧路径</a-select-option>
                    <a-select-option v-for="(path, index) in expansionRoots" :key="`series-${path}`" :value="index + 1">{{ path }}</a-select-option>
                  </a-select>
                </div>
              </div>
              <div class="storage-setting-help storage-setting-footer-help">
                <InfoCircleOutlined class="text-blue-400 mr-1 mt-0.5" />
                <span>仅影响后续新视频，不会移动已下载文件。</span>
              </div>
            </div>
          </div>
        </a-tab-pane>
      </a-tabs>
      <div class="bottom-config-actions">
        <a-space size="middle">
          <a-button type="primary" @click="onUpdate" v-if="componentDisabled">
            <SaveOutlined />修改配置
          </a-button>
          <a-button type="primary" danger @click="onSubmit" v-if="!componentDisabled">
            <CheckOutlined />确认保存
          </a-button>
          <a-button type="default" @click="onCancel" v-if="!componentDisabled">取消</a-button>
        </a-space>
      </div>
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
import { reactive, toRaw, ref, watch, onMounted, computed } from 'vue';
import type { UnwrapRef } from 'vue';
import { Form, Tooltip } from 'ant-design-vue';
import type { Rule } from 'ant-design-vue/es/form';
import type { FormInstance } from 'ant-design-vue';
import { useApiStore } from '@/store';
import { message, Modal } from 'ant-design-vue';
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
const isDockerDeployment = ref(false);
const dockerExpansionRoots = ref<string[]>([]);
const selectedDockerExpansionRoots = ref<string[]>([]);
const storageRootsLoading = ref(false);

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
  Cron: string;
  Id: string;
  BatchCount: string;
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
  DebugLogEnabled: boolean;
  VideoQuality: number;
  VideoEncoder: number;
  CloseNfo: boolean;
  SkipFileExistenceCheck: boolean;
  StorageExpansionRootsText: string;
  StorageMinFreeSpaceGb: number;
  ActiveCollectStorageRootIndex: number;
  ActiveFavoriteStorageRootIndex: number;
  ActiveFollowedStorageRootIndex: number;
  ActiveMixStorageRootIndex: number;
  ActiveSeriesStorageRootIndex: number;
}

// 表单初始数据
const formState: UnwrapRef<FormState> = reactive({
  Cron: '30',
  Id: '0',
  BatchCount: '10',
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
  DebugLogEnabled: true,
  VideoQuality: 0,
  VideoEncoder: 265,
  CloseNfo: false,
  SkipFileExistenceCheck: false,
  StorageExpansionRootsText: '',
  StorageMinFreeSpaceGb: 10,
  ActiveCollectStorageRootIndex: 0,
  ActiveFavoriteStorageRootIndex: 0,
  ActiveFollowedStorageRootIndex: 0,
  ActiveMixStorageRootIndex: 0,
  ActiveSeriesStorageRootIndex: 0,
});

const checkFileExistence = computed({
  get: () => !formState.SkipFileExistenceCheck,
  set: (enabled: boolean) => {
    formState.SkipFileExistenceCheck = !enabled;
  },
});

const expansionRoots = computed(() =>
  isDockerDeployment.value
    ? selectedDockerExpansionRoots.value
    : formState.StorageExpansionRootsText.split(';')
        .map((path) => path.trim())
        .filter((path) => path.length > 0)
);

const loadStorageDeploymentSettings = async () => {
  const api = useApiStore();
  try {
    const cachedVersion = api.getCachedVer();
    const version = cachedVersion?.code === 0 ? cachedVersion : await api.getVer(true);
    isDockerDeployment.value = version?.code === 0 && version.data?.deploy !== 'fnos';

    if (isDockerDeployment.value) {
      storageRootsLoading.value = true;
      const response = await api.apiGetDockerStorageExpansionRoots();
      if (response.code === 0) {
        dockerExpansionRoots.value = response.data || [];
        selectedDockerExpansionRoots.value = selectedDockerExpansionRoots.value.filter((path) =>
          dockerExpansionRoots.value.includes(path)
        );
      } else {
        message.error(response.message || '获取 Docker 扩展目录失败');
      }
    }
  } catch (error) {
    console.error('读取部署类型或 Docker 扩展目录失败:', error);
  } finally {
    storageRootsLoading.value = false;
  }
};

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
          Cron: String(res.data.cron ?? ''),
          Id: String(res.data.id ?? res.data.Id ?? formState.Id),
          BatchCount: String(res.data.batchCount ?? ''),
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
          DebugLogEnabled: res.data.debugLogEnabled ?? true,
          VideoQuality: res.data.videoQuality ?? 0,
          VideoEncoder: res.data.videoEncoder ?? 265,
          CloseNfo: res.data.closeNfo,
          SkipFileExistenceCheck: res.data.skipFileExistenceCheck || false,
          StorageExpansionRootsText: (() => {
            try {
              const parsedRoots: unknown = JSON.parse(res.data.storageExpansionRoots || '[]');
              const roots = Array.isArray(parsedRoots)
                ? parsedRoots.filter((path): path is string => typeof path === 'string')
                : [];
              selectedDockerExpansionRoots.value = roots;
              return roots.join('; ');
            } catch {
              return '';
            }
          })(),
          StorageMinFreeSpaceGb: res.data.storageMinFreeSpaceGb ?? 10,
          ActiveCollectStorageRootIndex: res.data.activeCollectStorageRootIndex || 0,
          ActiveFavoriteStorageRootIndex: res.data.activeFavoriteStorageRootIndex || 0,
          ActiveFollowedStorageRootIndex: res.data.activeFollowedStorageRootIndex || 0,
          ActiveMixStorageRootIndex: res.data.activeMixStorageRootIndex || 0,
          ActiveSeriesStorageRootIndex: res.data.activeSeriesStorageRootIndex || 0,
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
const movePriority = (index: number, direction: -1 | 1) => {
  const targetIndex = index + direction;
  if (targetIndex < 0 || targetIndex >= tagData.value.length) return;

  const [item] = tagData.value.splice(index, 1);
  tagData.value.splice(targetIndex, 0, item);
  updateTagSort();
};

onMounted(() => {
  void loadStorageDeploymentSettings();
  getConfig();
});
const onSubmit = () => {
  const cron = Number(formState.Cron);
  const batchCount = Number(formState.BatchCount);
  if (!/^\d+$/.test(formState.Cron) || !Number.isSafeInteger(cron) || cron < 15 || cron > 2147483647) {
    message.error('同步周期必须是大于等于 15 分钟的整数');
    return;
  }
  if (!/^\d+$/.test(formState.BatchCount) || !Number.isInteger(batchCount) || batchCount < 10 || batchCount > 40) {
    message.error('单次最多下载数量必须是 10 到 40 之间的整数');
    return;
  }

  formRef.value
    ?.validate()
    .then(() => {
      let fullTemplate = '';
      if (!formState.UperUseViedoTitle) {
        fullTemplate = formState.FollowedTitleTemplate.join(formState.FollowedTitleSeparator);
      }

      const submitData = {
        ...toRaw(formState),
        Cron: cron,
        BatchCount: batchCount,
        StorageExpansionRoots: JSON.stringify(expansionRoots.value),
        ActiveCollectStorageRootIndex: Math.min(formState.ActiveCollectStorageRootIndex, expansionRoots.value.length),
        ActiveFavoriteStorageRootIndex: Math.min(formState.ActiveFavoriteStorageRootIndex, expansionRoots.value.length),
        ActiveFollowedStorageRootIndex: Math.min(formState.ActiveFollowedStorageRootIndex, expansionRoots.value.length),
        ActiveMixStorageRootIndex: Math.min(formState.ActiveMixStorageRootIndex, expansionRoots.value.length),
        ActiveSeriesStorageRootIndex: Math.min(formState.ActiveSeriesStorageRootIndex, expansionRoots.value.length),
        FullFollowedTitleTemplate: fullTemplate,
        FollowedTitleTemplate: formState.FollowedTitleTemplate.join(''),
        PriorityLevel: JSON.stringify(tagData.value),
      };

      useApiStore()
        .apiUpdateConfig(submitData)
        .then((res) => {
          if (res.code === 0 && res.data !== false) {
            message.success('修改成功，同步任务将在5-10秒按新配置运行...');
            componentDisabled.value = true;
            getConfig();
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

/* ===== 系统配置页面：视觉美化覆盖 ===== */

/* 页面整体 */
.system-config-page {
  min-height: calc(100vh - 90px);
  border-radius: 16px !important;
  background: radial-gradient(circle at 100% 0, rgba(124, 58, 237, 0.055), transparent 28%), #f7f9fc !important;
}

.system-config-page :deep(.ant-card-body) {
  padding: 16px !important;
}

.system-config-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  align-items: start;
  gap: 14px;
}

/* 配置分组卡片 */
.form-section {
  position: relative;
  min-width: 0;
  margin: 0 !important;
  padding: 16px 18px 12px !important;
  overflow: hidden;
  border: 1px solid #e8edf3 !important;
  border-radius: 14px;
  background: linear-gradient(180deg, #ffffff 0%, #fbfcfd 100%);
  box-shadow: 0 5px 18px rgba(31, 45, 61, 0.045);
}

.form-section::before {
  content: '';
  position: absolute;
  inset: 0 auto 0 0;
  width: 3px;
  background: linear-gradient(180deg, #8b5cf6, #6366f1);
}

/* 分组标题 */
.section-title {
  position: relative;
  margin: 0 0 14px !important;
  padding: 0 0 10px !important;
  display: flex;
  align-items: center;
  gap: 8px;
  border-left: 0 !important;
  border-bottom: 1px solid #edf1f5;
  color: #25313b !important;
  font-size: 15px !important;
  line-height: 1.3;
  font-weight: 700 !important;
}

.section-title::before {
  content: '';
  width: 9px;
  height: 9px;
  border-radius: 3px;
  background: linear-gradient(135deg, #8b5cf6, #6366f1);
  box-shadow: 0 0 0 4px rgba(124, 58, 237, 0.09);
}

/* 表单项统一紧凑 */
.system-config-form :deep(.ant-form-item) {
  margin-bottom: 13px !important;
}

.system-config-form :deep(.ant-form-item:last-child) {
  margin-bottom: 0 !important;
}

.system-config-form :deep(.ant-form-item-label) {
  padding-right: 12px !important;
}

.system-config-form :deep(.ant-form-item-label > label) {
  height: 34px !important;
  color: #596574;
  font-size: 12px;
  line-height: 34px !important;
  font-weight: 600;
}

.system-config-form :deep(.ant-form-item-control-input) {
  min-height: 34px !important;
}

.system-config-form :deep(.ant-form-item-control-input-content) {
  min-width: 0;
}

/* 输入控件 */
.system-config-form :deep(.ant-input),
.system-config-form :deep(.ant-input-number),
.system-config-form :deep(.ant-select-selector) {
  min-height: 34px !important;
  border-color: #dfe5ec !important;
  border-radius: 8px !important;
  background: #fafbfc !important;
  box-shadow: none !important;
  transition: border-color 0.2s ease, background-color 0.2s ease, box-shadow 0.2s ease;
}

.system-config-form :deep(.ant-input:hover),
.system-config-form :deep(.ant-input-number:hover),
.system-config-form :deep(.ant-select:hover .ant-select-selector) {
  border-color: #aeb8c5 !important;
  background: #ffffff !important;
}

.system-config-form :deep(.ant-input:focus),
.system-config-form :deep(.ant-input-focused),
.system-config-form :deep(.ant-input-number-focused),
.system-config-form :deep(.ant-select-focused .ant-select-selector) {
  border-color: #7c3aed !important;
  background: #ffffff !important;
  box-shadow: 0 0 0 2px rgba(124, 58, 237, 0.09) !important;
}

.system-config-form :deep(.ant-input-number) {
  width: 180px;
}

.system-config-form :deep(.ant-input-number-input) {
  height: 32px;
}

.system-config-form :deep(.ant-select-selection-item),
.system-config-form :deep(.ant-select-selection-placeholder) {
  line-height: 32px !important;
}

.system-config-form :deep(.ant-select-multiple .ant-select-selector) {
  height: auto !important;
  min-height: 36px !important;
  padding: 3px 6px !important;
}

.system-config-form :deep(.ant-select-selection-item) {
  border-radius: 6px;
}

/* 禁用预览框 */
.system-config-form :deep(.ant-input-disabled) {
  color: #687382 !important;
  border-color: #e5e9ee !important;
  background: #f1f4f7 !important;
}

/* 开关统一主色 */
.system-config-form :deep(.ant-switch-checked) {
  background: #22a45a !important;
}

/* 单选按钮 */
.system-config-form :deep(.ant-radio-button-wrapper) {
  height: 34px;
  padding: 0 18px;
  color: #596574;
  border-color: #dfe5ec;
  line-height: 32px;
}

.system-config-form :deep(.ant-radio-group-small .ant-radio-button-wrapper) {
  height: 28px;
  padding: 0 12px;
  font-size: 13px;
  line-height: 26px;
}

.system-config-form :deep(.ant-radio-button-wrapper:first-child) {
  border-radius: 8px 0 0 8px;
}

.system-config-form :deep(.ant-radio-button-wrapper:last-child) {
  border-radius: 0 8px 8px 0;
}

.system-config-form :deep(.ant-radio-button-wrapper-checked:not(.ant-radio-button-wrapper-disabled)) {
  color: #ffffff;
  border-color: #7c3aed;
  background: #7c3aed;
  box-shadow: -1px 0 0 0 #7c3aed;
}

/* 说明文字改成轻量提示块 */
.system-config-form :deep(.flex.items-start.mt-1.text-sm.text-gray-500) {
  max-width: 100%;
  margin-top: 6px !important;
  padding: 7px 9px;
  display: flex;
  align-items: flex-start;
  border-radius: 8px;
  color: #7a8592 !important;
  background: #f6f8fa;
  font-size: 11px !important;
  line-height: 1.5 !important;
}

.system-config-form :deep(.flex.items-start.mt-1.text-sm.text-gray-500 .anticon) {
  margin-top: 2px !important;
  color: #7c3aed !important;
  flex-shrink: 0;
}

.system-config-form :deep(.flex.items-start.mt-1.text-sm.text-gray-500 span[style*='red']) {
  color: #df3f47 !important;
}

/* 拖拽优先级 */
.tag-drag-container {
  width: fit-content;
  max-width: 100%;
  padding: 3px 5px;
  border: 1px dashed #d9e0e8;
  border-radius: 8px;
  background: #f8fafb;
}

.tag-list {
  display: flex;
  flex-direction: row;
  flex-wrap: wrap;
  gap: 8px !important;
}

.priority-row {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  gap: 5px;
  flex: 0 0 auto;
  min-height: 27px;
  padding: 2px 5px;
  border-radius: 6px;
  color: #4b3680;
  background: rgba(124, 58, 237, 0.08);
  font-size: 12px;
}

.priority-label,
.priority-controls {
  display: flex;
  align-items: center;
}

.priority-label {
  gap: 5px;
}

.priority-index {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 18px;
  height: 18px;
  border-radius: 50%;
  color: #fff;
  background: #7c3aed;
  font-size: 11px;
  font-weight: 600;
}

.priority-controls {
  gap: 2px;
}

.priority-move-button {
  width: 20px;
  height: 20px;
  padding: 0 !important;
  color: #6d28d9 !important;
  font-size: 12px;
  line-height: 18px;
}

.preference-priority :deep(.ant-form-item-row) {
  align-items: center !important;
}

.priority-move-button:disabled {
  color: #b8b4c2 !important;
}

html.dark-mode .priority-row {
  color: #e2d8f5;
  background: rgba(139, 92, 246, 0.16);
}

html.dark-mode .priority-move-button:disabled {
  color: #666273 !important;
}

/* 音频按钮 */
.audio-check-wrapper {
  padding: 0 !important;
}

.audio-check-wrapper :deep(.ant-btn-primary) {
  height: 34px;
  border: 0;
  border-radius: 8px;
  background: linear-gradient(135deg, #8b5cf6, #6d28d9);
  box-shadow: 0 5px 13px rgba(109, 40, 217, 0.18);
}

.audio-check-wrapper :deep(.ant-btn-primary.audio-list-button) {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0;
  width: 44px;
  min-width: 44px;
  height: 24px;
  padding: 0;
  border-radius: 6px;
  font-size: 11px;
  line-height: 20px;
  box-shadow: 0 2px 6px rgba(109, 40, 217, 0.16);
}

.audio-check-wrapper :deep(.audio-list-button .anticon) {
  font-size: 12px;
}

/* 操作区横跨两列 */
.form-actions {
  grid-column: 1 / -1;
  margin: 0 !important;
  padding: 14px 18px;
  border: 1px solid #e8edf3;
  border-radius: 14px;
  background: #ffffff;
  box-shadow: 0 5px 18px rgba(31, 45, 61, 0.04);
}

.form-actions :deep(.ant-form-item-control) {
  max-width: none !important;
  margin-left: 0 !important;
}

.form-actions :deep(.ant-form-item-control-input-content) {
  display: flex;
  justify-content: flex-end;
}

.form-actions :deep(.ant-btn) {
  height: 36px;
  padding: 0 16px;
  border-radius: 9px;
  font-size: 12px;
}

.form-actions :deep(.ant-btn-primary:not(.ant-btn-dangerous)) {
  border-color: #7c3aed;
  background: #7c3aed;
}

.form-actions :deep(.ant-btn-primary:not(.ant-btn-dangerous):hover) {
  border-color: #6d28d9;
  background: #6d28d9;
}

/* 导入导出悬浮按钮 */
.config-float-btn-container {
  right: 24px !important;
  bottom: 34px !important;
  width: 52px !important;
}

.main-float-btn {
  width: 52px !important;
  height: 52px !important;
  border: 0 !important;
  background: linear-gradient(135deg, #8b5cf6, #6d28d9) !important;
  box-shadow: 0 9px 24px rgba(109, 40, 217, 0.26) !important;
}

.float-sub-btn-wrapper {
  bottom: 62px !important;
  gap: 7px !important;
}

.sub-float-btn {
  width: 42px !important;
  height: 42px !important;
  border-color: #dedfea !important;
  background: #ffffff !important;
  box-shadow: 0 5px 15px rgba(31, 45, 61, 0.11) !important;
}

/* 音频弹窗 */
:deep(.audio-list-container .ant-list) {
  overflow: hidden;
  border-color: #e7ebf0;
  border-radius: 12px;
}

:deep(.audio-list-container .ant-list-item) {
  padding: 10px 12px !important;
}

:deep(.audio-list-container .ant-list-item:hover) {
  background: #f7f9fb !important;
}

.native-audio-player {
  width: 220px !important;
  height: 34px !important;
  border-color: #dfe5ec !important;
  border-radius: 8px !important;
  background: #f7f9fb !important;
}

/* 暗色主题 */
html.dark-mode .system-config-page {
  background: radial-gradient(circle at 100% 0, rgba(124, 58, 237, 0.12), transparent 28%), #141426 !important;
}

html.dark-mode .form-section,
html.dark-mode .form-actions {
  border-color: #303247 !important;
  background: linear-gradient(180deg, #1b1b31 0%, #18182c 100%);
  box-shadow: none;
}

html.dark-mode .section-title {
  color: #f0f1f5 !important;
  border-bottom-color: #303247;
}

html.dark-mode .system-config-form :deep(.ant-form-item-label > label) {
  color: #b9bbc8;
}

html.dark-mode .system-config-form :deep(.ant-input),
html.dark-mode .system-config-form :deep(.ant-input-number),
html.dark-mode .system-config-form :deep(.ant-select-selector) {
  color: #e7e8ef !important;
  border-color: #34364c !important;
  background: #202037 !important;
}

html.dark-mode .system-config-form :deep(.ant-input-disabled) {
  color: #a7a9b5 !important;
  border-color: #303247 !important;
  background: #18182c !important;
}

html.dark-mode .system-config-form :deep(.ant-radio-button-wrapper) {
  color: #b9bbc8;
  border-color: #34364c;
  background: #202037;
}

html.dark-mode .system-config-form :deep(.ant-radio-button-wrapper-checked:not(.ant-radio-button-wrapper-disabled)) {
  color: #ffffff;
  border-color: #7c3aed;
  background: #7c3aed;
}

html.dark-mode .system-config-form :deep(.flex.items-start.mt-1.text-sm.text-gray-500) {
  color: #9ea0ad !important;
  background: rgba(255, 255, 255, 0.045);
}

html.dark-mode .tag-drag-container {
  border-color: #3b3d54;
  background: rgba(255, 255, 255, 0.035);
}

html.dark-mode .sub-float-btn {
  color: #d9d9e3 !important;
  border-color: #34364c !important;
  background: #202037 !important;
}

html.dark-mode :deep(.audio-list-container .ant-list-item:hover) {
  background: rgba(255, 255, 255, 0.04) !important;
}

html.dark-mode .native-audio-player {
  border-color: #34364c !important;
  background: #202037 !important;
}

/* 响应式 */
@media (max-width: 1180px) {
  .system-config-form {
    grid-template-columns: 1fr;
  }

  .form-actions {
    grid-column: 1;
  }
}

@media (max-width: 768px) {
  .system-config-page :deep(.ant-card-body) {
    padding: 10px !important;
  }

  .form-section {
    padding: 14px 12px 10px !important;
    border-radius: 12px;
  }

  .system-config-form {
    gap: 10px;
  }

  .system-config-form :deep(.ant-form-item-label) {
    width: 112px !important;
    flex: 0 0 112px !important;
  }

  .system-config-form :deep(.ant-form-item-control) {
    max-width: calc(100% - 112px) !important;
  }

  .system-config-form :deep(.ant-input-number) {
    width: 100%;
  }

  .form-actions :deep(.ant-form-item-control-input-content) {
    justify-content: center;
  }

  .config-float-btn-container {
    right: 14px !important;
    bottom: 20px !important;
  }
}

@media (max-width: 520px) {
  .system-config-form :deep(.ant-form-item) {
    display: block;
  }

  .system-config-form :deep(.ant-form-item-label),
  .system-config-form :deep(.ant-form-item-control) {
    width: 100% !important;
    max-width: 100% !important;
    flex: none !important;
  }

  .system-config-form :deep(.ant-form-item-label) {
    padding: 0 0 4px !important;
    text-align: left !important;
  }

  .system-config-form :deep(.ant-form-item-label > label) {
    height: auto !important;
    line-height: 1.4 !important;
  }

  .form-actions :deep(.ant-space) {
    width: 100%;
    display: grid;
    grid-template-columns: 1fr;
  }

  .form-actions :deep(.ant-btn) {
    width: 100%;
  }

  .native-audio-player {
    width: 170px !important;
  }
}

/* ===== 表单标签压缩与标题模板单行优化 ===== */

/*
 * 桌面端缩短左侧标签宽度，让控件区域获得更多空间。
 * 覆盖 labelCol 的 150px 和各表单项 wrapper-col 的 span 限制。
 */
@media (min-width: 769px) {
  .system-config-form :deep(.ant-form-item-row) {
    flex-wrap: nowrap !important;
    align-items: flex-start;
  }

  .system-config-form :deep(.ant-form-item-label) {
    width: 160px !important;
    max-width: 160px !important;
    flex: 0 0 160px !important;
    padding-right: 10px !important;
  }

  .system-config-form :deep(.ant-form-item-label > label) {
    width: 100%;
    justify-content: flex-end;
    white-space: nowrap;
  }

  .system-config-form :deep(.ant-form-item-control) {
    width: auto !important;
    max-width: none !important;
    min-width: 0 !important;
    flex: 1 1 0 !important;
  }

  .system-config-form :deep(.ant-form-item-control-input),
  .system-config-form :deep(.ant-form-item-control-input-content) {
    width: 100%;
    min-width: 0;
  }

  /* 数字输入框保持合适宽度，不因右侧空间增大而拉满 */
  .system-config-form :deep(.ant-input-number) {
    width: 180px;
  }
}

/* 博主视频区域的三个标题规则控件占满右侧可用空间 */
.title-template-item :deep(.ant-select),
.title-separator-item :deep(.ant-input),
.title-preview-item :deep(.ant-input) {
  width: 100% !important;
  max-width: 100% !important;
}

.title-separator-item :deep(.ant-input) {
  width: 240px !important;
  max-width: 100% !important;
}

/*
 * 标题模板尽量保持单行。
 * 内容超出时在选择框内部横向滚动，不再把整个表单项撑成多行。
 */
.title-template-item :deep(.ant-select-multiple .ant-select-selector) {
  height: 38px !important;
  min-height: 38px !important;
  padding: 3px 7px !important;
  overflow: hidden;
}

.title-template-item :deep(.ant-select-selection-overflow) {
  width: 100%;
  min-width: 0;
  flex-wrap: nowrap !important;
  overflow-x: auto;
  overflow-y: hidden;
  scrollbar-width: thin;
  scrollbar-color: rgba(120, 126, 140, 0.28) transparent;
}

.title-template-item :deep(.ant-select-selection-overflow::-webkit-scrollbar) {
  height: 3px;
}

.title-template-item :deep(.ant-select-selection-overflow::-webkit-scrollbar-track) {
  background: transparent;
}

.title-template-item :deep(.ant-select-selection-overflow::-webkit-scrollbar-thumb) {
  border-radius: 999px;
  background: rgba(120, 126, 140, 0.28);
}

.title-template-item :deep(.ant-select-selection-overflow-item) {
  flex: 0 0 auto !important;
}

.title-template-item :deep(.ant-select-selection-item) {
  height: 26px !important;
  margin-top: 2px !important;
  margin-bottom: 2px !important;
  padding-inline-start: 7px !important;
  padding-inline-end: 5px !important;
  font-size: 11px;
  line-height: 24px !important;
  white-space: nowrap;
}

/* 提示信息同样使用完整右侧宽度 */
.title-template-item :deep(.flex.items-start.mt-1.text-sm.text-gray-500),
.title-separator-item :deep(.flex.items-start.mt-1.text-sm.text-gray-500) {
  width: 100%;
  box-sizing: border-box;
}

/* 暗色主题滚动条 */
html.dark-mode .title-template-item :deep(.ant-select-selection-overflow) {
  scrollbar-color: rgba(210, 212, 225, 0.28) transparent;
}

html.dark-mode .title-template-item :deep(.ant-select-selection-overflow::-webkit-scrollbar-thumb) {
  background: rgba(210, 212, 225, 0.28);
}

/* 平板端略微保留标签宽度，避免中文标题拥挤 */
@media (min-width: 769px) and (max-width: 1180px) {
  .system-config-form :deep(.ant-form-item-label) {
    width: 104px !important;
    max-width: 104px !important;
    flex-basis: 104px !important;
  }
}

/* 手机端继续沿用上下布局，不强制单行 */
@media (max-width: 520px) {
  .title-template-item :deep(.ant-select-multiple .ant-select-selector) {
    height: auto !important;
    min-height: 38px !important;
  }

  .title-template-item :deep(.ant-select-selection-overflow) {
    flex-wrap: wrap !important;
    overflow-x: visible;
  }
}

/* ===== 系统配置双列平衡布局 ===== */

/*
 * 只改变视觉排列：
 * 左列：任务调度、动态视频、下载偏好
 * 右列：博主视频、图文视频
 * 右列略宽，给标题模板和说明文字更多空间。
 */
.system-config-form {
  display: block !important;
}

.config-columns {
  display: grid;
  grid-template-columns:
    minmax(0, 0.94fr)
    minmax(0, 1.06fr);
  gap: 14px;
  align-items: start;
}

.config-column {
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.config-column .form-section {
  width: 100%;
  box-sizing: border-box;
}

.storage-config-section {
  width: 100%;
  margin-top: 14px;
}

.bottom-config-actions {
  display: flex;
  justify-content: center;
  width: 100%;
  padding: 16px 0 8px;
}

.storage-root-settings-grid {
  display: block;
}

.storage-root-settings-grid .storage-setting {
  display: grid;
  grid-template-columns: 150px minmax(220px, 340px) minmax(0, 1fr);
  align-items: center;
  column-gap: 10px;
  margin-bottom: 10px;
}

.storage-root-settings-grid .storage-setting-label {
  margin: 0;
  color: #475569;
  font-size: 13px;
  text-align: right;
}

.storage-root-settings-grid .storage-setting-help {
  grid-column: 3;
  margin-top: 0;
  padding: 7px 10px;
  border-left: 2px solid #c4b5fd;
  border-radius: 0 7px 7px 0;
  background: #f8fafc;
  font-size: 12px;
  line-height: 1.45;
}

.storage-root-settings-grid #storage-min-free-space {
  width: 100%;
  max-width: 340px;
}

.storage-root-settings-grid :deep(.ant-input),
.storage-root-settings-grid :deep(.ant-input-number),
.storage-root-settings-grid :deep(.ant-select-selector) {
  min-height: 38px;
  border-radius: 8px !important;
}

.storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selector),
.storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selection-overflow) {
  align-items: center;
}

.storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selector) {
  padding-top: 2px;
  padding-bottom: 2px;
}

.storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selection-item) {
  display: inline-flex;
  align-items: center;
  box-sizing: border-box;
  height: 26px;
  margin: 0 4px 0 0;
  padding: 0 7px;
  border: 1px solid #e2d8ff;
  border-radius: 6px;
  color: #6336b5;
  background: #f3efff;
  line-height: 1;
}

.storage-root-settings-grid :deep(.ant-select-selection-item-content),
.storage-root-settings-grid :deep(.ant-select-selection-item-remove) {
  display: inline-flex;
  align-items: center;
  line-height: 1;
}

.storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selection-item-remove) {
  margin-left: 5px;
  color: #8b75b5;
}

.storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selection-item-remove:hover) {
  color: #6d28d9;
}

.storage-active-roots-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(100%, 180px), 1fr));
  gap: 14px;
  align-items: start;
  margin-top: 12px;
}

.storage-active-roots-grid .storage-setting-label {
  margin-bottom: 6px;
  font-size: 12px;
}

.storage-active-roots-grid :deep(.ant-select-selector) {
  min-height: 36px;
  border-radius: 8px !important;
}

.storage-setting {
  min-width: 0;
}

.storage-setting-label {
  display: block;
  margin-bottom: 8px;
  color: #394150;
  font-weight: 600;
}

.storage-setting :deep(.ant-input-number),
.storage-setting :deep(.ant-input),
.storage-setting :deep(.ant-select) {
  width: 100%;
}

.storage-setting-help {
  display: flex;
  align-items: flex-start;
  gap: 4px;
  margin-top: 8px;
  padding: 7px 9px;
  border-radius: 7px;
  background: #f7f8fb;
  color: #8992a1;
  font-size: 12px;
  line-height: 1.45;
}

.storage-setting-help :deep(.anticon) {
  flex: 0 0 auto;
  margin-top: 2px;
}

.storage-setting-footer-help {
  margin-top: 4px;
}

@media (max-width: 1380px) {
  .storage-active-roots-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .storage-root-settings-grid .storage-setting {
    grid-template-columns: 105px minmax(0, 1fr);
  }

  .storage-root-settings-grid .storage-setting-help {
    grid-column: 2;
  }

  .storage-active-roots-grid {
    grid-template-columns: minmax(0, 1fr);
  }
}

/* 避免左右卡片因默认网格行高互相拉伸 */
.config-column-left .form-section,
.config-column-right .form-section {
  align-self: stretch;
}

/* 操作栏与上方内容保持紧凑衔接 */
.form-actions {
  margin-top: 14px !important;
  min-height: 58px;
  padding-top: 10px !important;
  padding-bottom: 10px !important;
}

/* 右侧标题模板区域继续优先获得可用空间 */
.config-column-right .title-template-item :deep(.ant-form-item-control),
.config-column-right .title-separator-item :deep(.ant-form-item-control),
.config-column-right .title-preview-item :deep(.ant-form-item-control) {
  min-width: 0 !important;
  max-width: none !important;
  flex: 1 1 0 !important;
}

.config-column-right .title-template-item :deep(.ant-select),
.config-column-right .title-preview-item :deep(.ant-input) {
  width: 100% !important;
}

/* 图文视频提示较长，适当压缩上下留白 */
.config-column-right .form-section:last-child :deep(.ant-form-item) {
  margin-bottom: 11px !important;
}

/* 下载偏好区域的提示文字更紧凑 */
.config-column-left .form-section:last-child :deep(.flex.items-start.mt-1.text-sm.text-gray-500) {
  padding-top: 6px;
  padding-bottom: 6px;
}

/* 暗色主题保持原有配色，只应用布局 */
html.dark-mode .config-columns {
  background: transparent;
}

html.dark-mode .storage-setting-label {
  color: #d1d5db;
}

html.dark-mode .storage-setting-help {
  background: rgba(255, 255, 255, 0.06);
  color: #9ca3af;
}

html.dark-mode .storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selection-item) {
  border-color: #4a3d67;
  color: #ddccff;
  background: rgba(139, 92, 246, 0.18);
}

html.dark-mode .storage-root-settings-grid :deep(.ant-select-multiple .ant-select-selection-item-remove) {
  color: #b7a4d8;
}

/* 中等屏幕改为等宽双列 */
@media (max-width: 1380px) and (min-width: 981px) {
  .config-columns {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

/* 小屏自动单列，顺序按左右列依次展示 */
@media (max-width: 980px) {
  .config-columns {
    grid-template-columns: 1fr;
  }

  .config-column {
    gap: 10px;
  }

  .config-column-right {
    margin-top: 0;
  }

  .form-actions {
    margin-top: 10px !important;
  }
}

/* ===== 保存按钮移动到右列底部 ===== */

/* 两列拉伸到相同高度，右列按钮才能与左列底部对齐 */
.config-columns {
  align-items: stretch !important;
}

.config-column-right {
  height: 100%;
}

/* 右列底部操作区 */
.right-column-actions {
  min-height: 66px;
  margin-top: auto;
  padding: 10px 14px;
  display: flex;
  align-items: center;
  justify-content: flex-end;
  box-sizing: border-box;
  border: 1px solid #e8edf3;
  border-radius: 14px;
  background: #ffffff;
  box-shadow: 0 5px 18px rgba(31, 45, 61, 0.04);
}

/* 清除旧的整行操作栏样式 */
.right-column-actions .form-actions {
  width: auto;
  min-height: 0;
  margin: 0 !important;
  padding: 0 !important;
  border: 0 !important;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
}

.right-column-actions .form-actions :deep(.ant-form-item-row) {
  display: block;
}

.right-column-actions .form-actions :deep(.ant-form-item-control) {
  width: auto !important;
  max-width: none !important;
  margin-left: 0 !important;
  flex: none !important;
}

.right-column-actions .form-actions :deep(.ant-form-item-control-input) {
  min-height: 36px !important;
}

.right-column-actions .form-actions :deep(.ant-form-item-control-input-content) {
  display: flex;
  justify-content: flex-end;
}

.right-column-actions .form-actions :deep(.ant-btn) {
  min-width: 96px;
}

/* 暗色主题沿用当前页面配色 */
html.dark-mode .right-column-actions {
  border-color: #303247;
  background: linear-gradient(180deg, #1b1b31 0%, #18182c 100%);
  box-shadow: none;
}

/* 单列布局时，操作区自然位于所有右列配置之后 */
@media (max-width: 980px) {
  .right-column-actions {
    min-height: 58px;
    margin-top: 0;
    padding: 9px 12px;
  }
}

@media (max-width: 520px) {
  .right-column-actions {
    justify-content: stretch;
  }

  .right-column-actions .form-actions,
  .right-column-actions .form-actions :deep(.ant-form-item-control),
  .right-column-actions .form-actions :deep(.ant-form-item-control-input-content),
  .right-column-actions .form-actions :deep(.ant-space) {
    width: 100% !important;
  }
}

/* ===== 右列底部按钮左对齐 ===== */

/* 操作区仍位于右侧列底部，但按钮从该区域左侧开始排列 */
.right-column-actions {
  justify-content: flex-start !important;
}

.right-column-actions .form-actions :deep(.ant-form-item-control-input-content) {
  justify-content: flex-start !important;
}

/* 编辑状态下多个按钮也保持左对齐 */
.right-column-actions .form-actions :deep(.ant-space) {
  justify-content: flex-start;
}

/* 手机端按钮仍然铺满，避免过窄 */
@media (max-width: 520px) {
  .right-column-actions {
    justify-content: stretch !important;
  }

  .right-column-actions .form-actions :deep(.ant-form-item-control-input-content) {
    justify-content: stretch !important;
  }
}

/* ===== 左右底部边缘对齐 ===== */

/*
 * 两列由同一个 Grid 行承载，强制左右列占满同一高度。
 * 左侧最后一个“下载偏好”卡片自动吸收少量剩余高度，
 * 与右侧保存按钮区域的底边严格对齐。
 */
.config-columns {
  align-items: stretch !important;
}

.config-column-left,
.config-column-right {
  height: 100%;
  min-height: 100%;
  box-sizing: border-box;
}

.config-column-left .form-section:last-child {
  flex: 1 1 auto;
}

.config-column-right .right-column-actions {
  margin-top: auto !important;
  align-self: stretch;
  box-sizing: border-box;
}

/* 清除两侧最后区域可能产生的额外底部间距 */
.config-column-left .form-section:last-child,
.config-column-right .right-column-actions {
  margin-bottom: 0 !important;
}

/* 单列布局时不强制拉伸，避免出现多余空白 */
@media (max-width: 980px) {
  .config-column-left,
  .config-column-right {
    height: auto;
    min-height: 0;
  }

  .config-column-left .form-section:last-child {
    flex: 0 0 auto;
  }
}

/* Settings are grouped by task; keep each tab compact and easy to scan. */
.settings-tabs :deep(.ant-tabs-nav) {
  margin-bottom: 14px;
  padding: 4px;
  border-radius: 12px;
  background: #f0f3f8;
}
.settings-tabs :deep(.ant-tabs-nav::before) {
  border-bottom: 0;
}
.settings-tabs :deep(.ant-tabs-tab) {
  flex: 1 1 0;
  justify-content: center;
  margin: 0;
  padding: 9px 12px;
  border-radius: 9px;
  font-weight: 600;
}
.settings-tabs :deep(.ant-tabs-nav-list) {
  display: flex;
  width: 100%;
}
.settings-tabs :deep(.ant-tabs-tab-active) {
  background: #ffffff;
  box-shadow: 0 1px 4px rgba(30, 41, 59, 0.12);
}
.settings-tabs :deep(.ant-tabs-ink-bar) {
  display: none;
}
.settings-tab-panel {
  display: block;
  align-items: start;
}
.settings-tab-panel--narrow {
  display: block;
}
.sync-settings-grid,
.download-preference-grid,
.filename-settings-grid {
  display: block;
}
.sync-settings-grid {
  display: block;
}
.settings-tab-panel :deep(.ant-form-item-row) {
  display: flex !important;
  flex-flow: row nowrap !important;
  align-items: flex-start;
  width: 100%;
}
.settings-tab-panel :deep(.ant-form-item-label) {
  width: 150px !important;
  max-width: 150px !important;
  flex: 0 0 150px !important;
  padding: 0 12px 0 0 !important;
  text-align: left !important;
}
.settings-tab-panel :deep(.ant-form-item-control) {
  width: auto !important;
  max-width: none !important;
  min-width: 0;
  flex: 1 1 0 !important;
  margin: 0 !important;
  padding: 0 !important;
}
.settings-tab-panel :deep(.ant-form-item-control-input) {
  width: 100%;
  min-width: 0;
}
.settings-tab-panel :deep(.ant-form-item-control-input-content) {
  display: flex !important;
  align-items: center;
  gap: 10px;
  text-align: left;
}
.settings-tab-panel :deep(.ant-form-item-control-input-content > .flex.items-start.mt-1.text-sm.text-gray-500) {
  flex: 1 1 0;
  min-width: 0;
  margin-top: 0 !important;
  white-space: normal;
}
.settings-tab-panel :deep(.ant-form-item-control-input-content > .ant-input) {
  max-width: 360px;
  flex: 0 1 360px;
}
.settings-tab-panel :deep(.sync-number-input) {
  width: 50px;
  max-width: 50px !important;
  flex: 0 0 50px !important;
}
.settings-tab-panel :deep(.ant-form-item-control-input-content > .ant-select) {
  width: min(360px, 45%) !important;
  max-width: 360px;
  flex: 0 1 360px;
}
.settings-tab-panel :deep(.ant-form-item-control-input-content > .ant-input-number) {
  width: 60px;
  flex: 0 0 60px;
}
.settings-tab-panel :deep(.ant-form-item-control-input-content > .ant-radio-group),
.settings-tab-panel :deep(.ant-form-item-control-input-content > .ant-switch),
.settings-tab-panel :deep(.ant-form-item-control-input-content > .audio-check-wrapper),
.settings-tab-panel :deep(.ant-form-item-control-input-content > .tag-drag-container) {
  flex: 0 1 auto;
}
.filename-settings :deep(.title-template-item .ant-form-item-control-input-content),
.filename-settings :deep(.title-separator-item .ant-form-item-control-input-content) {
  display: grid !important;
  grid-template-columns: minmax(0, 80%) minmax(0, 1fr);
  align-items: center;
  gap: 8px 10px;
}
.filename-settings :deep(.title-separator-item .ant-form-item-control-input-content) {
  grid-template-columns: minmax(0, 200px) minmax(0, 1fr);
}
.filename-settings :deep(.title-template-item .ant-form-item-control-input-content > .ant-select),
.filename-settings :deep(.title-separator-item .ant-form-item-control-input-content > .ant-input) {
  grid-column: 1;
  width: 100% !important;
  max-width: 100%;
  flex: none;
}
.filename-settings :deep(.title-preview-item .ant-form-item-control-input-content > .ant-input) {
  width: 80% !important;
  max-width: 80%;
  flex: 0 0 80%;
}
.filename-settings
  :deep(.title-template-item .ant-form-item-control-input-content > .flex.items-start.mt-1.text-sm.text-gray-500),
.filename-settings
  :deep(.title-separator-item .ant-form-item-control-input-content > .flex.items-start.mt-1.text-sm.text-gray-500) {
  grid-column: 1 / -1;
  width: 100%;
  margin-top: 0 !important;
}
.sync-only-new,
.preference-priority,
.title-template-item {
  grid-column: 1 / -1;
}
.settings-tab-panel--download {
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.system-config-form :deep(.anticon-info-circle) {
  color: #7c3aed !important;
}
.media-switch-grid {
  display: contents;
}
.media-download-settings {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 14px;
}
.media-download-settings > .section-title {
  grid-column: 1 / -1;
}
.media-download-settings > div {
  display: contents;
}
.media-download-settings :deep(.ant-form-item) {
  min-width: 0;
}
@media (max-width: 680px) {
  .media-download-settings {
    grid-template-columns: minmax(0, 1fr);
  }
}
.filename-preview-grid {
  display: block;
}
.settings-tab-panel > .form-section {
  width: 100%;
  min-width: 0;
  margin: 0 !important;
}
.settings-tab-panel > .storage-config-section {
  width: 100%;
}
.bottom-config-actions {
  justify-content: center;
}
@media (max-width: 980px) {
  .settings-tab-panel {
    display: block;
  }
}
@media (max-width: 680px) {
  .settings-tabs :deep(.ant-tabs-tab) {
    padding: 8px 4px;
    font-size: 12px;
  }
  .sync-only-new,
  .preference-priority,
  .title-template-item {
    width: 100%;
  }
  .settings-tab-panel :deep(.ant-form-item-row) {
    display: flex !important;
    flex-flow: row nowrap !important;
  }
  .settings-tab-panel :deep(.ant-form-item-label) {
    width: 112px !important;
    max-width: 112px !important;
    flex-basis: 112px !important;
  }
}
</style>
