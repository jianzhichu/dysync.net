import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';
import Components from 'unplugin-vue-components/vite';
import { AntDesignVueResolver } from 'unplugin-vue-components/resolvers';
import { AntdvLessPlugin, AntdvModifyVars } from 'stepin/lib/style/plugins';
import viteCompression from 'vite-plugin-compression';
const timestamp = new Date().getTime();
const prodRollupOptions = {
  output: {
    chunkFileNames: (chunk) => {
      return 'assets/' + chunk.name + '.[hash]' + '.' + timestamp + '.js';
    },
    assetFileNames: (asset) => {
      const name = asset.name;
      if (name && (name.endsWith('.css') || name.endsWith('.js'))) {
        const names = name.split('.');
        const extname = names.splice(names.length - 1, 1)[0];
        return `assets/${names.join('.')}.[hash].${timestamp}.${extname}`;
      }
      return 'assets/' + asset.name;
    },
  },
};
// vite 配置
export default ({ command, mode }) => {
  // 获取环境变量
  const env = loadEnv(mode, process.cwd());
  return defineConfig({
    server: {
      host: "0.0.0.0",
      cors: true,
      port: 8001,
      open: false, //自动打开
      proxy: {
        '/api': {
          target: env.VITE_API_URL,
          ws: true,
          changeOrigin: true,
          rewrite: (path) => path.replace(/^\//, ''),
        },
      },
      hmr: true,
    },

    resolve: {
      alias: {
        '@': path.resolve(__dirname, 'src'),
      },
    },
    esbuild: {
      jsxFactory: 'h',
      jsxFragment: 'Fragment',
    },
    build: {
      sourcemap: false,
      chunkSizeWarningLimit: 2048,
      rollupOptions: mode === 'production' ? prodRollupOptions : {},
    },
    plugins: [
      vue({
        template: {
          transformAssetUrls: {
            img: ['src'],
            'a-avatar': ['src'],
            'stepin-view': ['logo-src', 'presetThemeList'],
            'a-card': ['cover'],
          },
        },
      }),
      Components({
        resolvers: [AntDesignVueResolver({ importStyle: mode === 'development' ? false : 'less' })],
      }),
      // viteCompression({
      //   verbose: true, // 是否在控制台中输出压缩结果
      //   disable: false,
      //   threshold: 10240, // 如果体积大于阈值，将被压缩，单位为b，体积过小时请不要压缩，以免适得其反
      //   algorithm: 'gzip', // 压缩算法，可选['gzip'，' brotliccompress '，'deflate '，'deflateRaw']
      //   ext: '.gz',
      //   deleteOriginFile: true // 源文件压缩后是否删除(我为了看压缩后的效果，先选择了true)
      // }),
    ],
    css: {
      preprocessorOptions: {
        less: {
          plugins: [AntdvLessPlugin],

          modifyVars: {
            ...AntdvModifyVars, // 保留默认变量（可选，按需添加）
            '@primary-color': '#722ed1',
            // 你的自定义主色调（核心：覆盖默认值）
            // 其他需要修改的变量
            // '@link-color': '#722ed1',
          },
          javascriptEnabled: true,
        },
      },
    },
    // base: env.VITE_BASE_URL,
  });
};
