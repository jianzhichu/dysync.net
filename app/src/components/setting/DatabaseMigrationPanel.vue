<template>
  <a-card class="database-panel" :bordered="false" :loading="statusLoading">
    <div class="database-panel-header">
      <div class="database-heading">
        <span class="database-heading-icon">DB</span>
        <div>
          <div class="database-title-row">
            <span class="database-title">数据库迁移</span>
            <span class="background-badge">后台执行</span>
          </div>
          <div class="database-description">在 SQLite、MySQL 与 PostgreSQL 之间安全迁移业务数据</div>
        </div>
      </div>
      <a-space class="database-actions">
        <a-button :loading="progressLoading" :disabled="refreshRemaining > 0" @click="refreshProgress">
          {{ refreshRemaining > 0 ? `${refreshRemaining} 秒后可刷新` : '刷新迁移进度' }}
        </a-button>
        <a-button type="primary" :disabled="jobRunning" @click="openMigration">迁移数据库</a-button>
      </a-space>
    </div>

    <div class="database-overview">
      <div class="overview-item">
        <span class="overview-label">当前数据库</span>
        <span class="overview-value"><i class="status-dot"></i>{{ status.dbType }}</span>
      </div>
      <div class="overview-divider"></div>
      <div class="overview-item">
        <span class="overview-label">最近任务</span>
        <span class="overview-value">{{ stateText }}</span>
      </div>
      <div class="overview-divider"></div>
      <div class="overview-item">
        <span class="overview-label">迁移记录</span>
        <span class="overview-value">{{ migrationStatus.history.length }} 条</span>
      </div>
    </div>

    <div v-if="migrationStatus.state !== 'Idle'" class="migration-progress">
      <div class="migration-progress-header">
        <span>
          <a-tag :color="stateColor">{{ stateText }}</a-tag>
          {{ migrationStatus.message || '等待刷新迁移状态' }}
        </span>
        <span v-if="migrationStatus.updatedAt" class="updated-at">
          更新时间：{{ formatTime(migrationStatus.updatedAt) }}
        </span>
      </div>
      <a-progress
        :percent="Number(migrationStatus.progressPercent || 0)"
        :status="migrationStatus.state === 'Failed' ? 'exception' : migrationStatus.state === 'Succeeded' ? 'success' : 'active'"
      />
      <div class="migration-progress-detail">
        <span>目标：{{ migrationStatus.targetDbType || '-' }}</span>
        <span>数据：{{ migrationStatus.migratedRows || 0 }} / {{ migrationStatus.totalRows || 0 }} 行</span>
        <span>表：{{ migrationStatus.completedTables || 0 }} / {{ migrationStatus.tableCount || 0 }}</span>
        <span v-if="migrationStatus.currentTable">当前表：{{ migrationStatus.currentTable }}</span>
      </div>
    </div>
    <div v-if="migrationStatus.history.length" class="migration-history">
      <button type="button" class="history-toggle" @click="showHistory = !showHistory">
        <span>迁移记录（{{ migrationStatus.history.length }}）</span>
        <span>{{ showHistory ? '收起' : '展开' }}</span>
      </button>
      <div v-if="showHistory" class="migration-history-list">
        <div v-for="item in migrationStatus.history" :key="item.jobId" class="migration-history-item">
          <a-tag :color="item.state === 'Succeeded' ? 'green' : 'red'">
            {{ item.state === 'Succeeded' ? '成功' : '失败' }}
          </a-tag>
          <span>{{ item.sourceDbType }} → {{ item.targetDbType }}</span>
          <span>{{ item.migratedRows || 0 }} / {{ item.totalRows || 0 }} 行</span>
          <span>{{ formatTime(item.completedAt) }}</span>
        </div>
      </div>
    </div>
  </a-card>
  <a-modal
    v-model:visible="visible"
    :width="720"
    :confirm-loading="migrating"
    ok-text="确认并开始后台迁移"
    cancel-text="暂不迁移"
    :mask-closable="false"
    :keyboard="!migrating"
    :body-style="{ padding: '14px 24px 16px' }"
    wrap-class-name="database-migration-modal"
    @ok="migrate"
  >
    <template #title>
      <div class="modal-title-wrap">
        <span class="modal-title-icon">⇄</span>
        <div>
          <div class="modal-title">迁移数据库</div>
          <div class="modal-subtitle">{{ status.dbType }} → {{ form.dbType }} · 创建可追踪的后台迁移任务</div>
        </div>
      </div>
    </template>

    <a-alert type="info" show-icon message="系统会暂停新同步任务并等待运行中的任务结束，再在后台迁移；你可以关闭弹窗后手动刷新进度。" class="migration-alert" />
    <a-form layout="vertical" class="migration-form">
      <DatabaseConfigFields
        variant="migration"
        v-model:database-type="form.dbType"
        v-model:host="form.host"
        v-model:port="form.port"
        v-model:user-name="form.userName"
        v-model:password="form.password"
        v-model:database-name="form.databaseName"
      />
    </a-form>
  </a-modal>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { message } from 'ant-design-vue';
import { useApiStore } from '@/store';
import DatabaseConfigFields from './DatabaseConfigFields.vue';
const statusLoading = ref(false);
const progressLoading = ref(false);
const migrating = ref(false);
const visible = ref(false);
const showHistory = ref(false);
const status = reactive({ dbType: 'Sqlite', canMigrate: false });
const migrationStatus = reactive({
  state: 'Idle', targetDbType: '', currentTable: '', tableCount: 0, completedTables: 0,
  totalRows: 0, migratedRows: 0, progressPercent: 0, message: '', updatedAt: '',
  refreshIntervalSeconds: 5, history: [] as any[],
});
const refreshRemaining = ref(0);
let refreshTimer: ReturnType<typeof setInterval> | null = null;
const form = reactive({ dbType: 'MySql', host: '', port: 3306, userName: '', password: '', databaseName: '' });
const jobRunning = computed(() => ['Queued', 'Running'].includes(migrationStatus.state));
const stateText = computed(() => ({
  Queued: '排队中', Running: '迁移中', Succeeded: '已完成', Failed: '失败',
} as Record<string, string>)[migrationStatus.state] || '暂无任务');
const stateColor = computed(() => ({
  Queued: 'orange', Running: 'blue', Succeeded: 'green', Failed: 'red',
} as Record<string, string>)[migrationStatus.state] || 'default');
const loadStatus = async () => {
  statusLoading.value = true;
  try {
    const response = await useApiStore().GetDatabaseStatus();
    if (response.code === 0 && response.data) Object.assign(status, response.data);
  } finally { statusLoading.value = false; }
};
const startRefreshCooldown = (seconds = migrationStatus.refreshIntervalSeconds || 5) => {
  refreshRemaining.value = Math.max(1, Number(seconds));
  if (refreshTimer) clearInterval(refreshTimer);
  refreshTimer = setInterval(() => {
    refreshRemaining.value = Math.max(0, refreshRemaining.value - 1);
    if (refreshRemaining.value === 0 && refreshTimer) {
      clearInterval(refreshTimer);
      refreshTimer = null;
    }
  }, 1000);
};
const refreshProgress = async () => {
  if (progressLoading.value || refreshRemaining.value > 0) return;
  progressLoading.value = true;
  try {
    const response = await useApiStore().GetDatabaseMigrationStatus();
    if (response.code !== 0) {
      const retryAfter = response.data?.retryAfterSeconds;
      if (retryAfter) startRefreshCooldown(retryAfter);
      return void message.warning(response.message || '获取迁移进度失败');
    }
    if (response.data) {
      Object.assign(migrationStatus, response.data);
      startRefreshCooldown(response.data.refreshIntervalSeconds || 5);
      if (response.data.state === 'Succeeded') await loadStatus();
    }
  } catch (error: any) {
    message.error(error?.message || '获取迁移进度失败');
  } finally {
    progressLoading.value = false;
  }
};
const openMigration = () => {
  const targetType = status.dbType === 'Sqlite' ? 'MySql' : 'Sqlite';
  Object.assign(form, { dbType: targetType, host: '', port: 3306, userName: '', password: '', databaseName: '' });
  visible.value = true;
};
const migrate = async () => {
  if (form.dbType !== 'Sqlite' && (!form.host.trim() || !form.userName.trim() || !form.password)) {
    return void message.warning('请完整填写 Host、端口、账号和密码');
  }
  migrating.value = true;
  try {
    const response = await useApiStore().MigrateDatabase({ ...form });
    if (response.code !== 0) return void message.error(response.message || '数据库迁移失败', 8);
    visible.value = false;
    if (response.data) Object.assign(migrationStatus, response.data);
    startRefreshCooldown(response.data?.refreshIntervalSeconds || 5);
    message.success('迁移任务已提交到后台，请稍后刷新查看进度', 5);
  } catch (error: any) {
    message.error(error?.response?.data?.message || error?.message || '数据库迁移失败', 8);
  } finally { migrating.value = false; }
};
const formatTime = (value: string) => value ? new Date(value).toLocaleString() : '-';
onMounted(async () => {
  await loadStatus();
  await refreshProgress();
});
onBeforeUnmount(() => {
  if (refreshTimer) clearInterval(refreshTimer);
});
</script>

<style scoped>
.database-panel { margin: 0 0 16px; overflow: hidden; border: 1px solid #e8edf3; border-radius: 14px; box-shadow: 0 8px 28px rgba(32, 56, 85, .06); }
.database-panel-header { display: flex; align-items: center; justify-content: space-between; gap: 20px; }
.database-heading { display: flex; align-items: center; gap: 13px; }
.database-heading-icon { display: inline-flex; align-items: center; justify-content: center; width: 42px; height: 42px; border-radius: 12px; background: linear-gradient(145deg, #1677ff, #69a9ff); color: #fff; font-size: 12px; font-weight: 800; letter-spacing: -.5px; box-shadow: 0 6px 14px rgba(22, 119, 255, .2); }
.database-title-row { display: flex; align-items: center; gap: 8px; }
.database-title { color: #182235; font-size: 17px; font-weight: 700; }
.background-badge { padding: 2px 7px; border-radius: 999px; background: #eef6ff; color: #1677ff; font-size: 11px; }
.database-description { margin-top: 3px; color: #7e8998; font-size: 13px; }
.database-overview { display: flex; align-items: stretch; margin-top: 18px; padding: 13px 16px; border: 1px solid #edf0f4; border-radius: 11px; background: #fafbfd; }
.overview-item { display: flex; min-width: 130px; flex: 1; flex-direction: column; gap: 3px; }
.overview-label { color: #929ba8; font-size: 11px; }
.overview-value { display: flex; align-items: center; gap: 7px; color: #273246; font-size: 14px; font-weight: 600; }
.status-dot { width: 7px; height: 7px; border-radius: 50%; background: #52c41a; box-shadow: 0 0 0 3px rgba(82, 196, 26, .12); }
.overview-divider { width: 1px; margin: 0 20px; background: #e7ebf0; }
.migration-progress { margin-top: 16px; padding: 15px; border: 1px solid #e5edf8; border-radius: 11px; background: #fbfdff; }
.migration-progress-header { display: flex; align-items: center; justify-content: space-between; gap: 12px; margin-bottom: 8px; }
.migration-progress-detail { display: flex; flex-wrap: wrap; gap: 8px 20px; color: #6b7280; font-size: 13px; }
.migration-history { margin-top: 12px; }
.history-toggle { display: flex; width: 100%; align-items: center; justify-content: space-between; padding: 10px 12px; border: 0; border-radius: 9px; background: #f7f9fb; color: #526071; font-size: 13px; cursor: pointer; }
.history-toggle:hover { background: #f0f5fb; color: #1677ff; }
.migration-history-list { max-height: 240px; overflow-y: auto; }
.migration-history-item { display: grid; grid-template-columns: 64px minmax(150px, 1fr) minmax(110px, auto) minmax(150px, auto); align-items: center; gap: 10px; padding: 7px 0; border-bottom: 1px dashed #f0f0f0; color: #6b7280; font-size: 13px; }
.updated-at { color: #9ca3af; font-size: 12px; }
.modal-title-wrap { display: flex; align-items: center; gap: 11px; }
.modal-title-icon { display: inline-flex; align-items: center; justify-content: center; width: 34px; height: 34px; border-radius: 10px; background: #eaf3ff; color: #1677ff; font-size: 20px; }
.modal-title { color: #192235; font-size: 16px; font-weight: 700; }
.modal-subtitle { margin-top: 1px; color: #8a94a2; font-size: 11px; font-weight: 400; }
.migration-alert { margin-bottom: 12px; border-radius: 9px; }
.migration-form { padding: 2px 2px 0; }
@media (max-width: 640px) {
  .database-panel-header, .migration-progress-header { align-items: flex-start; flex-direction: column; }
  .database-actions { width: 100%; }
  .database-overview { flex-direction: column; gap: 10px; }
  .overview-divider { width: 100%; height: 1px; margin: 0; }
  .migration-history-item { grid-template-columns: 64px 1fr; }
}
</style>
