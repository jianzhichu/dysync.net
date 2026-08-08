<template>
  <div class="database-switch-page">
    <div class="page-heading">
      <div>
        <h1>数据库切换</h1>
        <p>管理数据库迁移、切换进度和历史记录</p>
      </div>
      <div class="page-note">
        <span class="note-dot"></span>
        支持 SQLite、MySQL、PostgreSQL
      </div>
    </div>

    <DatabaseMigrationPanel />

    <div class="migration-notes">
      <div class="note-item">
        <span class="note-index">01</span>
        <div><strong>原库保留</strong><small>迁移不会删除或覆盖当前数据库</small></div>
      </div>
      <div class="note-item">
        <span class="note-index">02</span>
        <div><strong>自动停 Job</strong><small>等待运行任务结束后再开始迁移</small></div>
      </div>
      <div class="note-item">
        <span class="note-index">03</span>
        <div><strong>自动切换</strong><small>成功后服务重启并连接目标数据库</small></div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import DatabaseMigrationPanel from '@/components/setting/DatabaseMigrationPanel.vue';
</script>

<style scoped>
.database-switch-page { min-height: calc(100vh - 90px); padding: 4px 0 24px; }
.page-heading { display: flex; align-items: flex-end; justify-content: space-between; gap: 20px; margin-bottom: 18px; }
.page-heading h1 { margin: 0; color: #172033; font-size: 24px; font-weight: 750; letter-spacing: -.4px; }
.page-heading p { margin: 5px 0 0; color: #818b99; font-size: 13px; }
.page-note { display: flex; align-items: center; gap: 8px; padding: 7px 11px; border: 1px solid #e4eaf1; border-radius: 999px; background: #fff; color: #667284; font-size: 12px; }
.note-dot { width: 7px; height: 7px; border-radius: 50%; background: #52c41a; box-shadow: 0 0 0 3px rgba(82, 196, 26, .12); }
.migration-notes { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-top: 14px; }
.note-item { display: flex; align-items: center; gap: 12px; padding: 14px 16px; border: 1px solid #e8edf3; border-radius: 11px; background: rgba(255, 255, 255, .72); }
.note-index { color: #9fc4f5; font-size: 18px; font-weight: 800; }
.note-item div { display: flex; min-width: 0; flex-direction: column; }
.note-item strong { color: #334057; font-size: 13px; }
.note-item small { margin-top: 2px; color: #929ba8; font-size: 11px; }
@media (max-width: 760px) {
  .page-heading { align-items: flex-start; flex-direction: column; }
  .migration-notes { grid-template-columns: 1fr; }
}
</style>
