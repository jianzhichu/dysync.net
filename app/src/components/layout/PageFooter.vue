<script lang="ts" setup>
import { GithubOutlined, CopyrightOutlined } from '@ant-design/icons-vue';
import { ref, onMounted } from 'vue';

// 响应式变量存储年份文本
const yearText = ref('');

onMounted(() => {
  const currentYear = new Date().getFullYear();
  const startYear = 2025;

  if (currentYear <= startYear) {
    yearText.value = startYear.toString();
  } else {
    yearText.value = `${startYear}-${currentYear}`;
  }
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
        <div class="social-links">
          <a href="https://github.com/jianzhichu/dysync.net" target="_blank" rel="noopener noreferrer" aria-label="访问我们的 GitHub 仓库" class="social-link">
            <GithubOutlined />
          </a>
          <a href="https://gitee.com/deathvicky/dysync.net" target="_blank" rel="noopener noreferrer" aria-label="访问我们的 Gitee 仓库" class="social-link">
            <img class="gitee-icon" src="@/assets/gitee.svg" alt="Gitee" />
          </a>
        </div>
      </div>
    </div>
  </footer>
</template>

<style scoped lang="less">
.page-footer {
  // 使用 :root 变量，便于主题切换
  --footer-text-color: var(--text-color-secondary, #999);
  --footer-hover-color: var(--primary-color, #1890ff);

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