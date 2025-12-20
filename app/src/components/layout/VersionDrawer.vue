<!-- components/VersionDrawer.vue -->
<script lang="ts" setup>
import { ref, watch, onMounted } from 'vue';
import { Drawer } from 'ant-design-vue';
import { CloseOutlined, CopyOutlined, CheckCircleOutlined } from '@ant-design/icons-vue';
import { useApiStore } from '@/store';
import { message } from 'ant-design-vue';

// ✅ 移除 defineProps/defineEmits 导入，直接使用
const props = defineProps<{
  visible: boolean;
  currentVersion?: string; // 接收当前版本号
}>();

const emit = defineEmits<{
  (e: 'close'): void;
}>();

// 组件挂载时获取配置
onMounted(() => {});

const getVersions = () => {
  useApiStore()
    .CheckTag()
    .then((res) => {
      if (res.code == 1) {
        dyVersions.value = res.data;
      } else {
        message.error(res.message);
      }
    });
};

// 版本数据
const dyVersions = ref<string[]>([]);

// 关闭抽屉
const handleClose = () => {
  emit('close');
};

// 复制版本号
const copyVersion = (version: string) => {
  navigator.clipboard
    .writeText(version)
    .then(() => {
      message.success(`已复制版本: ${version}`);
    })
    .catch(() => {
      message.error('复制失败，请手动复制');
    });
};

// 调试：监听 visible 变化，确认父组件传递的状态是否正确
console.log('抽屉初始状态：', props.visible);
watch(
  () => props.visible,
  (val) => {
    console.log('抽屉 visible 变化：', val);
    if (val) {
      getVersions();
    }
  }
);
</script>

<template>
  <!-- 强制添加样式，确保不被隐藏 -->
  <a-drawer title="版本列表" placement="right" :visible="props.visible" @close="handleClose" :width="450" :mask="true" :mask-closable="true" :z-index="9999" class="version-drawer">
    <div class="version-list">
      <div v-for="(tag, index) in dyVersions" :key="index" class="version-item">
        <div class="version-info">
          {{ tag }}
          <!-- 当前版本标记 -->
          <span v-if="props.currentVersion === tag" class="current-version-tag">
            <CheckCircleOutlined class="current-icon" />
            当前版本
          </span>
        </div>
        <!-- 复制按钮 -->
        <a-tooltip title="复制版本号" placement="top">
          <div class="copy-btn" @click.stop="copyVersion(tag)">
            <CopyOutlined />
          </div>
        </a-tooltip>
      </div>
      <div v-if="dyVersions.length === 0" class="empty-tip">暂无版本数据</div>
    </div>

    <!-- <div class="close-btn" @click="handleClose">
      <CloseOutlined /> 关闭版本列表
    </div> -->
  </a-drawer>
</template>

<style scoped lang="less">
/* 强制显示抽屉容器，避免被全局样式隐藏 */
.version-drawer {
  display: block !important;
}

:deep(.ant-drawer-content-wrapper) {
  display: block !important;
  visibility: visible !important;
  opacity: 1 !important;
  z-index: 9999 !important; /* 确保层级最高 */
}

.version-list {
  padding: 20px;
  line-height: 2;
}

.version-item {
  padding: 10px 0;
  border-bottom: 1px solid #f0f0f0;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  justify-content: space-between;
  align-items: center;

  &:hover {
    background: #f5f5f5;
  }
}

.version-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

/* 当前版本标记样式 */
.current-version-tag {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #1890ff;
  background: #e6f7ff;
  padding: 2px 8px;
  border-radius: 12px;
}

.current-icon {
  font-size: 12px;
}

/* 复制按钮样式 */
.copy-btn {
  color: #999;
  font-size: 16px;
  cursor: pointer;
  transition: color 0.2s;
  padding: 4px;
  border-radius: 4px;

  &:hover {
    color: #1890ff;
    background: #f0f9ff;
  }
}

.empty-tip {
  text-align: center;
  color: #999;
  padding: 40px 0;
}

.close-btn {
  position: absolute;
  bottom: 24px;
  left: 50%;
  transform: translateX(-50%);
  color: #1890ff;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 14px;
}

/* 调整tooltip样式，避免被遮挡 */
:deep(.ant-tooltip) {
  z-index: 10000 !important;
}
</style>