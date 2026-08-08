<template>
  <a-form-item label="数据持久化类型">
    <a-select :value="databaseType" @change="changeType">
      <a-select-option value="Sqlite">SQLite（默认）</a-select-option>
      <a-select-option value="MySql">MySQL</a-select-option>
      <a-select-option value="PostgreSql">PostgreSQL</a-select-option>
    </a-select>
    <div class="database-tip">SQLite 无需额外配置；外部数据库名称可选填，留空默认使用 dysync。</div>
  </a-form-item>

  <template v-if="databaseType !== 'Sqlite'">
    <a-form-item label="Host" required>
      <a-input :value="host" placeholder="数据库服务器地址或容器服务名" @input="updateText('host', $event)" />
    </a-form-item>
    <a-form-item label="Port" required>
      <a-input-number :value="port" :min="1" :max="65535" style="width: 100%" @change="updatePort" />
    </a-form-item>
    <a-form-item label="账号" required>
      <a-input :value="userName" autocomplete="off" placeholder="数据库账号" @input="updateText('userName', $event)" />
    </a-form-item>
    <a-form-item label="密码" required>
      <a-input-password :value="password" autocomplete="new-password" placeholder="数据库密码" @input="updateText('password', $event)" />
    </a-form-item>
    <a-form-item label="数据库名（选填）">
      <a-input :value="databaseName" placeholder="留空则使用 dysync" @input="updateText('databaseName', $event)" />
      <div class="database-tip">留空使用 dysync；不存在时自动创建。仅支持字母、数字、下划线和短横线。</div>
    </a-form-item>
    <div class="database-tip warning">连接配置仅保存在服务端 db/database.json，请限制该文件的读取权限。</div>
  </template>
</template>

<script lang="ts" setup>
const props = defineProps<{
  databaseType: string;
  host: string;
  port: number;
  userName: string;
  password: string;
  databaseName: string;
}>();

const emit = defineEmits<{
  (event: 'update:databaseType', value: string): void;
  (event: 'update:host', value: string): void;
  (event: 'update:port', value: number): void;
  (event: 'update:userName', value: string): void;
  (event: 'update:password', value: string): void;
  (event: 'update:databaseName', value: string): void;
}>();

const changeType = (value: string) => {
  emit('update:databaseType', value);
  emit('update:port', value === 'PostgreSql' ? 5432 : 3306);
};

const updateText = (field: 'host' | 'userName' | 'password' | 'databaseName', event: Event) => {
  const value = (event.target as HTMLInputElement).value;
  if (field === 'host') emit('update:host', value);
  if (field === 'userName') emit('update:userName', value);
  if (field === 'password') emit('update:password', value);
  if (field === 'databaseName') emit('update:databaseName', value);
};

const updatePort = (value: number | null) => {
  emit('update:port', value ?? (props.databaseType === 'PostgreSql' ? 5432 : 3306));
};
</script>

<style scoped>
.database-tip { margin: 6px 0 14px; color: #6b7280; font-size: 13px; line-height: 1.5; }
.database-tip.warning { color: #d46b08; }
</style>
