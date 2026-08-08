<template>
  <a-card class="database-panel" size="small" :loading="statusLoading">
    <div class="database-panel-row">
      <div>
        <div class="database-title">数据库持久化</div>
        <div class="database-description">
          当前数据库：<a-tag color="blue">{{ status.dbType }}</a-tag>
          <span>可将当前数据库的业务数据迁移到新的 SQLite、MySQL 或 PostgreSQL 数据库。</span>
        </div>
      </div>
      <a-button type="primary" @click="openMigration">迁移数据库</a-button>
    </div>
  </a-card>
  <a-modal v-model:visible="visible" title="一键迁移数据库" :confirm-loading="migrating" ok-text="开始迁移" cancel-text="取消" :mask-closable="false" @ok="migrate">
    <a-alert type="warning" show-icon message="迁移成功后将切换到目标数据库并立即热重启后台服务，以后仍可继续迁移。请勿在迁移期间关闭页面或启动同步任务。" class="migration-alert" />
    <a-form layout="vertical">
      <DatabaseConfigFields
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
import { onMounted, reactive, ref } from 'vue';
import { message, Modal } from 'ant-design-vue';
import { useApiStore } from '@/store';
import DatabaseConfigFields from './DatabaseConfigFields.vue';
const statusLoading = ref(false);
const migrating = ref(false);
const visible = ref(false);
const status = reactive({ dbType: 'Sqlite', canMigrate: false });
const form = reactive({ dbType: 'MySql', host: '', port: 3306, userName: '', password: '', databaseName: '' });
const loadStatus = async () => {
  statusLoading.value = true;
  try {
    const response = await useApiStore().GetDatabaseStatus();
    if (response.code === 0 && response.data) Object.assign(status, response.data);
  } finally { statusLoading.value = false; }
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
  Modal.confirm({
    title: '确认迁移并切换数据库连接？',
    content: form.dbType === 'Sqlite'
      ? '系统会创建一个新的 SQLite 文件并迁移当前业务数据。原数据库不会删除，迁移成功后后台服务会立即重启。'
      : '目标数据库的业务表必须为空。迁移成功后将使用目标数据库，后台服务会立即重启。',
    okText: '确认迁移', cancelText: '取消',
    async onOk() {
      migrating.value = true;
      try {
        const response = await useApiStore().MigrateDatabase({ ...form });
        if (response.code !== 0) return void message.error(response.message || '数据库迁移失败', 8);
        visible.value = false;
        message.success(`迁移完成，共迁移 ${response.data?.rowCount ?? 0} 行，服务正在重启`, 5);
        setTimeout(() => window.location.reload(), 4000);
      } catch (error: any) {
        message.error(error?.response?.data?.message || error?.message || '数据库迁移失败', 8);
      } finally { migrating.value = false; }
    },
  });
};
onMounted(loadStatus);
</script>

<style scoped>
.database-panel { margin: 0 0 12px; }
.database-panel-row { display: flex; align-items: center; justify-content: space-between; gap: 20px; }
.database-title { margin-bottom: 6px; font-size: 16px; font-weight: 600; }
.database-description { color: #6b7280; }
.migration-alert { margin-bottom: 18px; }
@media (max-width: 640px) { .database-panel-row { align-items: flex-start; flex-direction: column; } }
</style>
