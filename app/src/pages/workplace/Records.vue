<template>
  <div class="workplace grid grid-rows-none gap-4 mt-xxs">
    <div class="project-list grid grid-cols-24 gap-4">
      <records class="col-span-12 xlx:col-span-7 xxlx:col-span-8 drop-shadow-sm" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { reactive } from 'vue';
import Records from './RecordTable.vue';
import { useUnbounded } from '@/utils/useTheme';

useUnbounded();
</script>

<style scoped lang="less">
.workplace {
}
</style>
