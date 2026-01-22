// vite.config.ts
import { defineConfig, loadEnv } from "file:///E:/code/dysync/app/node_modules/vite/dist/node/index.js";
import vue from "file:///E:/code/dysync/app/node_modules/@vitejs/plugin-vue/dist/index.mjs";
import path from "path";
import Components from "file:///E:/code/dysync/app/node_modules/unplugin-vue-components/dist/vite.mjs";
import { AntDesignVueResolver } from "file:///E:/code/dysync/app/node_modules/unplugin-vue-components/dist/resolvers.mjs";
import { AntdvLessPlugin, AntdvModifyVars } from "file:///E:/code/dysync/app/node_modules/stepin/lib/style/plugins/index.js";
var __vite_injected_original_dirname = "E:\\code\\dysync\\app";
var timestamp = (/* @__PURE__ */ new Date()).getTime();
var prodRollupOptions = {
  output: {
    chunkFileNames: (chunk) => {
      return "assets/" + chunk.name + ".[hash]." + timestamp + ".js";
    },
    assetFileNames: (asset) => {
      const name = asset.name;
      if (name && (name.endsWith(".css") || name.endsWith(".js"))) {
        const names = name.split(".");
        const extname = names.splice(names.length - 1, 1)[0];
        return `assets/${names.join(".")}.[hash].${timestamp}.${extname}`;
      }
      return "assets/" + asset.name;
    }
  }
};
var vite_config_default = ({ command, mode }) => {
  const env = loadEnv(mode, process.cwd());
  return defineConfig({
    server: {
      host: "0.0.0.0",
      cors: true,
      port: 8001,
      open: false,
      //自动打开
      proxy: {
        "/api": {
          target: env.VITE_API_URL,
          ws: true,
          changeOrigin: true,
          rewrite: (path2) => path2.replace(/^\//, "")
        }
      },
      hmr: true
    },
    resolve: {
      alias: {
        "@": path.resolve(__vite_injected_original_dirname, "src")
      }
    },
    esbuild: {
      jsxFactory: "h",
      jsxFragment: "Fragment"
    },
    build: {
      sourcemap: false,
      chunkSizeWarningLimit: 2048,
      rollupOptions: mode === "production" ? prodRollupOptions : {}
    },
    plugins: [
      vue({
        template: {
          transformAssetUrls: {
            img: ["src"],
            "a-avatar": ["src"],
            "stepin-view": ["logo-src", "presetThemeList"],
            "a-card": ["cover"]
          }
        }
      }),
      Components({
        resolvers: [AntDesignVueResolver({ importStyle: mode === "development" ? false : "less" })]
      })
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
            ...AntdvModifyVars,
            // 保留默认变量（可选，按需添加）
            "@primary-color": "#722ed1"
            // 你的自定义主色调（核心：覆盖默认值）
            // 其他需要修改的变量
            // '@link-color': '#722ed1',
          },
          javascriptEnabled: true
        }
      }
    }
    // base: env.VITE_BASE_URL,
  });
};
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcudHMiXSwKICAic291cmNlc0NvbnRlbnQiOiBbImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJFOlxcXFxjb2RlXFxcXGR5c3luY1xcXFxhcHBcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkU6XFxcXGNvZGVcXFxcZHlzeW5jXFxcXGFwcFxcXFx2aXRlLmNvbmZpZy50c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vRTovY29kZS9keXN5bmMvYXBwL3ZpdGUuY29uZmlnLnRzXCI7aW1wb3J0IHsgZGVmaW5lQ29uZmlnLCBsb2FkRW52IH0gZnJvbSAndml0ZSc7XHJcbmltcG9ydCB2dWUgZnJvbSAnQHZpdGVqcy9wbHVnaW4tdnVlJztcclxuaW1wb3J0IHBhdGggZnJvbSAncGF0aCc7XHJcbmltcG9ydCBDb21wb25lbnRzIGZyb20gJ3VucGx1Z2luLXZ1ZS1jb21wb25lbnRzL3ZpdGUnO1xyXG5pbXBvcnQgeyBBbnREZXNpZ25WdWVSZXNvbHZlciB9IGZyb20gJ3VucGx1Z2luLXZ1ZS1jb21wb25lbnRzL3Jlc29sdmVycyc7XHJcbmltcG9ydCB7IEFudGR2TGVzc1BsdWdpbiwgQW50ZHZNb2RpZnlWYXJzIH0gZnJvbSAnc3RlcGluL2xpYi9zdHlsZS9wbHVnaW5zJztcclxuaW1wb3J0IHZpdGVDb21wcmVzc2lvbiBmcm9tICd2aXRlLXBsdWdpbi1jb21wcmVzc2lvbic7XHJcbmNvbnN0IHRpbWVzdGFtcCA9IG5ldyBEYXRlKCkuZ2V0VGltZSgpO1xyXG5jb25zdCBwcm9kUm9sbHVwT3B0aW9ucyA9IHtcclxuICBvdXRwdXQ6IHtcclxuICAgIGNodW5rRmlsZU5hbWVzOiAoY2h1bmspID0+IHtcclxuICAgICAgcmV0dXJuICdhc3NldHMvJyArIGNodW5rLm5hbWUgKyAnLltoYXNoXScgKyAnLicgKyB0aW1lc3RhbXAgKyAnLmpzJztcclxuICAgIH0sXHJcbiAgICBhc3NldEZpbGVOYW1lczogKGFzc2V0KSA9PiB7XHJcbiAgICAgIGNvbnN0IG5hbWUgPSBhc3NldC5uYW1lO1xyXG4gICAgICBpZiAobmFtZSAmJiAobmFtZS5lbmRzV2l0aCgnLmNzcycpIHx8IG5hbWUuZW5kc1dpdGgoJy5qcycpKSkge1xyXG4gICAgICAgIGNvbnN0IG5hbWVzID0gbmFtZS5zcGxpdCgnLicpO1xyXG4gICAgICAgIGNvbnN0IGV4dG5hbWUgPSBuYW1lcy5zcGxpY2UobmFtZXMubGVuZ3RoIC0gMSwgMSlbMF07XHJcbiAgICAgICAgcmV0dXJuIGBhc3NldHMvJHtuYW1lcy5qb2luKCcuJyl9LltoYXNoXS4ke3RpbWVzdGFtcH0uJHtleHRuYW1lfWA7XHJcbiAgICAgIH1cclxuICAgICAgcmV0dXJuICdhc3NldHMvJyArIGFzc2V0Lm5hbWU7XHJcbiAgICB9LFxyXG4gIH0sXHJcbn07XHJcbi8vIHZpdGUgXHU5MTREXHU3RjZFXHJcbmV4cG9ydCBkZWZhdWx0ICh7IGNvbW1hbmQsIG1vZGUgfSkgPT4ge1xyXG4gIC8vIFx1ODNCN1x1NTNENlx1NzNBRlx1NTg4M1x1NTNEOFx1OTFDRlxyXG4gIGNvbnN0IGVudiA9IGxvYWRFbnYobW9kZSwgcHJvY2Vzcy5jd2QoKSk7XHJcbiAgcmV0dXJuIGRlZmluZUNvbmZpZyh7XHJcbiAgICBzZXJ2ZXI6IHtcclxuICAgICAgaG9zdDogXCIwLjAuMC4wXCIsXHJcbiAgICAgIGNvcnM6IHRydWUsXHJcbiAgICAgIHBvcnQ6IDgwMDEsXHJcbiAgICAgIG9wZW46IGZhbHNlLCAvL1x1ODFFQVx1NTJBOFx1NjI1M1x1NUYwMFxyXG4gICAgICBwcm94eToge1xyXG4gICAgICAgICcvYXBpJzoge1xyXG4gICAgICAgICAgdGFyZ2V0OiBlbnYuVklURV9BUElfVVJMLFxyXG4gICAgICAgICAgd3M6IHRydWUsXHJcbiAgICAgICAgICBjaGFuZ2VPcmlnaW46IHRydWUsXHJcbiAgICAgICAgICByZXdyaXRlOiAocGF0aCkgPT4gcGF0aC5yZXBsYWNlKC9eXFwvLywgJycpLFxyXG4gICAgICAgIH0sXHJcbiAgICAgIH0sXHJcbiAgICAgIGhtcjogdHJ1ZSxcclxuICAgIH0sXHJcblxyXG4gICAgcmVzb2x2ZToge1xyXG4gICAgICBhbGlhczoge1xyXG4gICAgICAgICdAJzogcGF0aC5yZXNvbHZlKF9fZGlybmFtZSwgJ3NyYycpLFxyXG4gICAgICB9LFxyXG4gICAgfSxcclxuICAgIGVzYnVpbGQ6IHtcclxuICAgICAganN4RmFjdG9yeTogJ2gnLFxyXG4gICAgICBqc3hGcmFnbWVudDogJ0ZyYWdtZW50JyxcclxuICAgIH0sXHJcbiAgICBidWlsZDoge1xyXG4gICAgICBzb3VyY2VtYXA6IGZhbHNlLFxyXG4gICAgICBjaHVua1NpemVXYXJuaW5nTGltaXQ6IDIwNDgsXHJcbiAgICAgIHJvbGx1cE9wdGlvbnM6IG1vZGUgPT09ICdwcm9kdWN0aW9uJyA/IHByb2RSb2xsdXBPcHRpb25zIDoge30sXHJcbiAgICB9LFxyXG4gICAgcGx1Z2luczogW1xyXG4gICAgICB2dWUoe1xyXG4gICAgICAgIHRlbXBsYXRlOiB7XHJcbiAgICAgICAgICB0cmFuc2Zvcm1Bc3NldFVybHM6IHtcclxuICAgICAgICAgICAgaW1nOiBbJ3NyYyddLFxyXG4gICAgICAgICAgICAnYS1hdmF0YXInOiBbJ3NyYyddLFxyXG4gICAgICAgICAgICAnc3RlcGluLXZpZXcnOiBbJ2xvZ28tc3JjJywgJ3ByZXNldFRoZW1lTGlzdCddLFxyXG4gICAgICAgICAgICAnYS1jYXJkJzogWydjb3ZlciddLFxyXG4gICAgICAgICAgfSxcclxuICAgICAgICB9LFxyXG4gICAgICB9KSxcclxuICAgICAgQ29tcG9uZW50cyh7XHJcbiAgICAgICAgcmVzb2x2ZXJzOiBbQW50RGVzaWduVnVlUmVzb2x2ZXIoeyBpbXBvcnRTdHlsZTogbW9kZSA9PT0gJ2RldmVsb3BtZW50JyA/IGZhbHNlIDogJ2xlc3MnIH0pXSxcclxuICAgICAgfSksXHJcbiAgICAgIC8vIHZpdGVDb21wcmVzc2lvbih7XHJcbiAgICAgIC8vICAgdmVyYm9zZTogdHJ1ZSwgLy8gXHU2NjJGXHU1NDI2XHU1NzI4XHU2M0E3XHU1MjM2XHU1M0YwXHU0RTJEXHU4RjkzXHU1MUZBXHU1MzhCXHU3RjI5XHU3RUQzXHU2NzlDXHJcbiAgICAgIC8vICAgZGlzYWJsZTogZmFsc2UsXHJcbiAgICAgIC8vICAgdGhyZXNob2xkOiAxMDI0MCwgLy8gXHU1OTgyXHU2NzlDXHU0RjUzXHU3OUVGXHU1OTI3XHU0RThFXHU5NjA4XHU1MDNDXHVGRjBDXHU1QzA2XHU4OEFCXHU1MzhCXHU3RjI5XHVGRjBDXHU1MzU1XHU0RjREXHU0RTNBYlx1RkYwQ1x1NEY1M1x1NzlFRlx1OEZDN1x1NUMwRlx1NjVGNlx1OEJGN1x1NEUwRFx1ODk4MVx1NTM4Qlx1N0YyOVx1RkYwQ1x1NEVFNVx1NTE0RFx1OTAwMlx1NUY5N1x1NTE3Nlx1NTNDRFxyXG4gICAgICAvLyAgIGFsZ29yaXRobTogJ2d6aXAnLCAvLyBcdTUzOEJcdTdGMjlcdTdCOTdcdTZDRDVcdUZGMENcdTUzRUZcdTkwMDlbJ2d6aXAnXHVGRjBDJyBicm90bGljY29tcHJlc3MgJ1x1RkYwQydkZWZsYXRlICdcdUZGMEMnZGVmbGF0ZVJhdyddXHJcbiAgICAgIC8vICAgZXh0OiAnLmd6JyxcclxuICAgICAgLy8gICBkZWxldGVPcmlnaW5GaWxlOiB0cnVlIC8vIFx1NkU5MFx1NjU4N1x1NEVGNlx1NTM4Qlx1N0YyOVx1NTQwRVx1NjYyRlx1NTQyNlx1NTIyMFx1OTY2NChcdTYyMTFcdTRFM0FcdTRFODZcdTc3MEJcdTUzOEJcdTdGMjlcdTU0MEVcdTc2ODRcdTY1NDhcdTY3OUNcdUZGMENcdTUxNDhcdTkwMDlcdTYyRTlcdTRFODZ0cnVlKVxyXG4gICAgICAvLyB9KSxcclxuICAgIF0sXHJcbiAgICBjc3M6IHtcclxuICAgICAgcHJlcHJvY2Vzc29yT3B0aW9uczoge1xyXG4gICAgICAgIGxlc3M6IHtcclxuICAgICAgICAgIHBsdWdpbnM6IFtBbnRkdkxlc3NQbHVnaW5dLFxyXG5cclxuICAgICAgICAgIG1vZGlmeVZhcnM6IHtcclxuICAgICAgICAgICAgLi4uQW50ZHZNb2RpZnlWYXJzLCAvLyBcdTRGRERcdTc1NTlcdTlFRDhcdThCQTRcdTUzRDhcdTkxQ0ZcdUZGMDhcdTUzRUZcdTkwMDlcdUZGMENcdTYzMDlcdTk3MDBcdTZERkJcdTUyQTBcdUZGMDlcclxuICAgICAgICAgICAgJ0BwcmltYXJ5LWNvbG9yJzogJyM3MjJlZDEnLFxyXG4gICAgICAgICAgICAvLyBcdTRGNjBcdTc2ODRcdTgxRUFcdTVCOUFcdTRFNDlcdTRFM0JcdTgyNzJcdThDMDNcdUZGMDhcdTY4MzhcdTVGQzNcdUZGMUFcdTg5ODZcdTc2RDZcdTlFRDhcdThCQTRcdTUwM0NcdUZGMDlcclxuICAgICAgICAgICAgLy8gXHU1MTc2XHU0RUQ2XHU5NzAwXHU4OTgxXHU0RkVFXHU2NTM5XHU3Njg0XHU1M0Q4XHU5MUNGXHJcbiAgICAgICAgICAgIC8vICdAbGluay1jb2xvcic6ICcjNzIyZWQxJyxcclxuICAgICAgICAgIH0sXHJcbiAgICAgICAgICBqYXZhc2NyaXB0RW5hYmxlZDogdHJ1ZSxcclxuICAgICAgICB9LFxyXG4gICAgICB9LFxyXG4gICAgfSxcclxuICAgIC8vIGJhc2U6IGVudi5WSVRFX0JBU0VfVVJMLFxyXG4gIH0pO1xyXG59O1xyXG4iXSwKICAibWFwcGluZ3MiOiAiO0FBQWdQLFNBQVMsY0FBYyxlQUFlO0FBQ3RSLE9BQU8sU0FBUztBQUNoQixPQUFPLFVBQVU7QUFDakIsT0FBTyxnQkFBZ0I7QUFDdkIsU0FBUyw0QkFBNEI7QUFDckMsU0FBUyxpQkFBaUIsdUJBQXVCO0FBTGpELElBQU0sbUNBQW1DO0FBT3pDLElBQU0sYUFBWSxvQkFBSSxLQUFLLEdBQUUsUUFBUTtBQUNyQyxJQUFNLG9CQUFvQjtBQUFBLEVBQ3hCLFFBQVE7QUFBQSxJQUNOLGdCQUFnQixDQUFDLFVBQVU7QUFDekIsYUFBTyxZQUFZLE1BQU0sT0FBTyxhQUFrQixZQUFZ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
