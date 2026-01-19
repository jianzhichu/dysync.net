<script lang="ts" setup>
import { GithubOutlined, CopyrightOutlined } from '@ant-design/icons-vue';
import { ref, onMounted } from 'vue';
import { useApiStore } from '@/store';

// 响应式变量存储年份文本
const yearText = ref('');
const currentVersion = ref(); // 已存在的版本号响应式变量
onMounted(() => {
  const currentYear = new Date().getFullYear();
  const startYear = 2025;

  if (currentYear <= startYear) {
    yearText.value = startYear.toString();
  } else {
    yearText.value = `${startYear}-${currentYear}`;
  }
  useApiStore()
    .getVer()
    .then((r) => {
      if (r.code == 0) {
        currentVersion.value = r.data; // 接口返回后赋值版本号
      }
    });
});
</script>

<template>
  <footer class="page-footer">
    <div class="footer-content">
      <!-- 版权信息容器 -->
      <div class="copyright">
        <CopyrightOutlined class="icon-copyright" />
        <span>{{ yearText }} 抖小云 dysync.net. </span>

        <!-- 将社交链接移动到这里 -->
        <!-- <div class="social-links">
          <a href="https://github.com/jianzhichu/dysync.net" target="_blank" rel="noopener noreferrer" aria-label="访问我们的 GitHub 仓库" class="social-link">
            <GithubOutlined />
          </a>
          <a href="https://gitee.com/deathvicky/dysync.net" target="_blank" rel="noopener noreferrer" aria-label="访问我们的 Gitee 仓库" class="social-link">
            <img class="gitee-icon" src="@/assets/gitee.svg" alt="Gitee" />
          </a>
        </div> -->

        <!-- 新增：显示当前版本号，增加样式分隔提升美观度 -->
        <span class="version-text" v-if="currentVersion">{{currentVersion.deploy}}_{{ currentVersion.tag }}</span>
      </div>
    </div>
  </footer>
</template>

<style scoped lang="less">
.page-footer {
  // 使用 :root 变量，便于主题切换
  --footer-text-color: var(--text-color-secondary, #999);
  --footer-hover-color: var(--primary-color, #1890ff);
  --version-text-color: var(--text-color-tertiary, #666);

  width: 100%;
  color: var(--footer-text-color);
  padding: 24px 0;
  font-size: 14px;
  line-height: 1.6;

  .footer-content {
    max-width: 1200px;
    margin: 0 auto;
    padding: 0 24px;
    text-align: center;
  }

  // 版权信息现在是一个父 flex 容器
  .copyright {
    display: flex;
    justify-content: center;
    align-items: center;
    // 增大 gap 以分隔文字和社交链接组
    gap: 10px;
    font-size: 13px;
    letter-spacing: 0.3px;
  }
  .version-text {
    // 核心修改1：白色文字（醒目，与绿底形成强烈对比）
    color: #ffffff;
    font-size: 12px;
    font-weight: 500;
    padding: 2px 6px; // 微调内边距，让绿底区域更饱满，视觉更舒适
    // 核心修改2：柔和绿色背景（两种可选方案，不刺眼、显高级）
    // 方案1：推荐！清新柔和绿（适配大多数页面，不张扬，高级感足）
    background-color: #722ed1;
    // 方案2：稍浅薄荷绿（更明亮，醒目度稍高，适合活泼风格页面）
    // background-color: #73d13d;
    border-radius: 4px; // 适当增大圆角，更显圆润柔和
    // 核心修改3：调整光效为白色光晕（匹配白字，提升质感，不突兀）
    // text-shadow: 0 0 3px rgba(255, 255, 255, 0.8), 0 0 6px rgba(255, 255, 255, 0.4);
    // 可选优化：轻微暗角阴影，让绿底更有层次感，不扁平
    // box-shadow: 0 1px 2px rgba(82, 196, 26, 0.2);
    transition: all 0.3s ease;
  }
  // 社交链接组自身也是一个 flex 容器
  .social-links {
    display: flex;
    // 为两个图标之间设置间距
    gap: 16px;
    // 移除了原来的 margin-bottom

    .social-link {
      color: inherit;
      transition: color 0.3s ease, transform 0.2s ease;
      display: inline-flex;
      opacity: 0.8;
      align-items: center;
      // 移除了图标和文字的 gap，因为现在没有文字了

      &:hover {
        color: var(--footer-hover-color);
        opacity: 1;
        transform: translateY(-2px);
      }

      svg,
      img {
        width: 20px;
        height: 20px;
        display: block;
        flex-shrink: 0;
      }
    }
  }

  .gitee-icon {
    width: 14px !important;
    height: 14px !important;
  }
}
</style>