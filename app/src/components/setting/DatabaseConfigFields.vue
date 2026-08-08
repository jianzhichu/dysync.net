<template>
  <template v-if="variant === 'migration'">
    <div class="field-section">
      <div class="section-heading">
        <span class="section-index">1</span>
        <div>
          <div class="section-title">选择目标数据库</div>
          <div class="section-description">请选择本次迁移完成后要使用的持久化类型</div>
        </div>
      </div>
      <div class="database-type-grid">
        <button
          v-for="item in databaseTypes"
          :key="item.value"
          type="button"
          class="database-type-card"
          :class="{ selected: databaseType === item.value }"
          @click="changeType(item.value)"
        >
          <span class="database-type-mark">{{ item.mark }}</span>
          <span class="database-type-content">
            <strong>{{ item.label }}</strong>
            <small>{{ item.description }}</small>
          </span>
          <span class="database-type-check">✓</span>
        </button>
      </div>
    </div>

    <div class="field-section connection-section">
      <div class="section-heading">
        <span class="section-index">2</span>
        <div>
          <div class="section-title">{{ databaseType === 'Sqlite' ? '确认存储方式' : '填写连接信息' }}</div>
          <div class="section-description">
            {{ databaseType === 'Sqlite' ? '系统将创建一个新的 SQLite 文件，不覆盖历史数据库' : '凭据只会保存在服务端持久化目录' }}
          </div>
        </div>
      </div>

      <div v-if="databaseType === 'Sqlite'" class="sqlite-note">
        <span class="sqlite-note-icon">✓</span>
        <div>
          <strong>无需额外连接配置</strong>
          <p>迁移成功后会切换到新文件，原数据库仍然保留，可用于备份或恢复。</p>
        </div>
      </div>

      <div v-else class="connection-fields">
        <div class="field-grid field-grid-host">
          <a-form-item label="服务器地址" required>
            <a-input :value="host" placeholder="IP 地址或容器服务名" @input="updateText('host', $event)" />
          </a-form-item>
          <a-form-item label="端口" required>
            <a-input-number :value="port" :min="1" :max="65535" style="width: 100%" @change="updatePort" />
          </a-form-item>
        </div>
        <div class="field-grid">
          <a-form-item label="数据库账号" required>
            <a-input :value="userName" autocomplete="off" placeholder="请输入账号" @input="updateText('userName', $event)" />
          </a-form-item>
          <a-form-item label="数据库密码" required>
            <a-input-password :value="password" autocomplete="new-password" placeholder="请输入密码" @input="updateText('password', $event)" />
          </a-form-item>
        </div>
        <a-form-item label="目标数据库名称">
          <a-input :value="databaseName" placeholder="留空默认使用 dysync" @input="updateText('databaseName', $event)" />
          <div class="database-tip">数据库不存在时会自动创建；名称仅支持字母、数字、下划线和短横线。</div>
        </a-form-item>
        <div class="credential-note">
          <span>🔒</span>
          <span>连接凭据仅写入服务端 <code>db/database.json</code>，不会返回到浏览器。</span>
        </div>
      </div>
    </div>
  </template>

  <template v-else>
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
</template>

<script lang="ts" setup>
const props = defineProps<{
  databaseType: string;
  host: string;
  port: number;
  userName: string;
  password: string;
  databaseName: string;
  variant?: 'default' | 'migration';
}>();

const databaseTypes = [
  { value: 'Sqlite', label: 'SQLite', mark: 'S', description: '单文件 · 免配置' },
  { value: 'MySql', label: 'MySQL', mark: 'M', description: '外部服务 · 通用' },
  { value: 'PostgreSql', label: 'PostgreSQL', mark: 'P', description: '外部服务 · 稳健' },
];

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
.field-section { padding: 0 0 12px; }
.connection-section { padding-top: 12px; border-top: 1px solid #edf0f4; }
.section-heading { display: flex; align-items: flex-start; gap: 10px; margin-bottom: 10px; }
.section-index { display: inline-flex; align-items: center; justify-content: center; flex: 0 0 25px; width: 25px; height: 25px; border-radius: 8px; background: #e8f1ff; color: #1677ff; font-size: 12px; font-weight: 700; }
.section-title { color: #172033; font-size: 15px; font-weight: 650; }
.section-description { margin-top: 2px; color: #87909f; font-size: 12px; }
.database-type-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 10px; }
.database-type-card { position: relative; display: flex; align-items: center; gap: 9px; min-width: 0; padding: 9px 10px; border: 1px solid #dfe4ea; border-radius: 10px; background: #fff; color: inherit; text-align: left; cursor: pointer; transition: all .2s ease; }
.database-type-card:hover { border-color: #91caff; transform: translateY(-1px); box-shadow: 0 6px 18px rgba(22, 119, 255, .08); }
.database-type-card.selected { border-color: #1677ff; background: #f5f9ff; box-shadow: 0 0 0 2px rgba(22, 119, 255, .08); }
.database-type-mark { display: inline-flex; align-items: center; justify-content: center; flex: 0 0 30px; width: 30px; height: 30px; border-radius: 8px; background: #eef2f7; color: #526071; font-size: 12px; font-weight: 800; }
.database-type-card.selected .database-type-mark { background: #1677ff; color: #fff; }
.database-type-content { display: flex; min-width: 0; flex-direction: column; }
.database-type-content strong { color: #222b3a; font-size: 14px; }
.database-type-content small { overflow: hidden; color: #8a94a3; font-size: 11px; text-overflow: ellipsis; white-space: nowrap; }
.database-type-check { position: absolute; top: 7px; right: 8px; display: none; color: #1677ff; font-size: 12px; font-weight: 800; }
.database-type-card.selected .database-type-check { display: block; }
.connection-fields { padding: 0 35px; }
.connection-fields :deep(.ant-form-item) { margin-bottom: 10px; }
.connection-fields :deep(.ant-form-item-label) { padding-bottom: 3px; }
.field-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 14px; }
.field-grid-host { grid-template-columns: minmax(0, 1fr) 150px; }
.sqlite-note { display: flex; align-items: flex-start; gap: 10px; margin: 0 35px; padding: 11px 13px; border: 1px solid #b7ebc6; border-radius: 10px; background: #f6ffed; }
.sqlite-note-icon { display: inline-flex; align-items: center; justify-content: center; flex: 0 0 28px; width: 28px; height: 28px; border-radius: 50%; background: #52c41a; color: #fff; font-weight: 700; }
.sqlite-note strong { color: #245b19; }
.sqlite-note p { margin: 4px 0 0; color: #5f7160; font-size: 12px; line-height: 1.6; }
.connection-fields .database-tip { margin: 3px 0 0; font-size: 12px; }
.credential-note { display: flex; gap: 8px; margin-top: 2px; padding: 8px 10px; border-radius: 8px; background: #f7f8fa; color: #737d8c; font-size: 11px; }
.credential-note code { color: #596579; }
@media (max-width: 640px) {
  .database-type-grid, .field-grid, .field-grid-host { grid-template-columns: 1fr; }
  .connection-fields { padding: 0; }
  .sqlite-note { margin: 0; }
}
</style>
