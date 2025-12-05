<script lang="ts" setup>
import { PropType, watch, computed } from 'vue';
import useModelValue from '@/utils/useModelValue';
import cloneDeep from 'lodash/cloneDeep';
import { storeToRefs } from 'pinia';
import { useThemeStore, ThemeProvider } from 'stepin/es/theme-provider';

export type Type = 'day' | 'night';

const props = defineProps({
  value: { type: String as PropType<Type> },
  nightColor: { type: String, default: '#1a1a2e' },
});

const emit = defineEmits<{
  (e: 'update:value', value: Type): void;
}>();

const STORAGE_KEY = 'theme-mode';

const getInitialValue = () => {
  if (typeof window !== 'undefined') {
    const cachedValue = localStorage.getItem(STORAGE_KEY);
    if (cachedValue === 'day' || cachedValue === 'night') {
      return cachedValue;
    }
  }
  return props.value || 'day';
};

const { value: _value } = useModelValue(
  () => props.value,
  (val) => emit('update:value', val),
  getInitialValue()
);

const switcher: { [key in Type]: Type } = {
  day: 'night',
  night: 'day',
};

const { theme } = storeToRefs(useThemeStore());
let cachedMiddleColors = {};

// --- 修正点 1: 函数定义移到 watch 之前 ---
const updateHtmlClass = (mode: Type) => {
  const html = document.documentElement;
  if (mode === 'night') {
    html.classList.add('dark-mode');
  } else {
    html.classList.remove('dark-mode');
  }
};

// 监听 theme bg-base 变化，用于同步外部修改
watch(
  () => theme.value?.color?.middle?.['bg-base'],
  (newBgColor) => {
    if (newBgColor) {
      if (newBgColor === props.nightColor) {
        _value.value = 'night';
      } else {
        _value.value = 'day';
      }
    }
  }
);

// --- 修正点 2: watch 现在可以安全地调用 updateHtmlClass ---
watch(
  _value,
  (newMode) => {
    // console.log('mode switched to:', newMode);
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, newMode);
    }
    updateHtmlClass(newMode);
  },
  { immediate: true }
);

// --- 修正点 3: 惰性缓存白天模式颜色 ---
const colorCfg = computed(() => {
  if (_value.value === 'day') {
    if (Object.keys(cachedMiddleColors).length === 0) {
      cachedMiddleColors = cloneDeep(theme.value?.color?.middle || {});
    }
    return { middle: cachedMiddleColors };
  }
  return { middle: { 'bg-base': props.nightColor } };
});
</script>

<template>
  <ThemeProvider is-root :color="colorCfg">
    <div @click="() => (_value = switcher[_value])" class="bg-fill-2 day-night-switch hover:border-border relative border-border-2 text-lg rounded-full border border-solid flex items-center">
      <div :class="`spot transition-[left] duration-300 h-full absolute rounded-full bg-container ${_value}`"></div>
      <IconFont :class="`day-night-switch-item ${_value === 'day' ? 'checked' : ''}`" name="icon-sun" />
      <IconFont :class="`day-night-switch-item ${_value === 'night' ? 'checked' : ''}`" name="icon-moono" />
    </div>
  </ThemeProvider>
</template>

<style scoped lang="less">
.day-night-switch {
  .spot {
    width: calc(50% - 1px);
    z-index: 1;
    left: 0;
    &.night {
      left: calc(50% + 1px);
      @apply bg-layout;
    }
  }

  &-item {
    @apply z-20 bg-transparent p-xxs rounded-full text-disabled ~"last:ml-[2px]";
    &.checked {
      @apply text-text;
    }
  }
}
</style>