import { createRouter, createWebHashHistory } from 'vue-router';
import { reactive } from 'vue';
import routes from './routes';
import guards from './guards';

const router = createRouter(
  {
    history: createWebHashHistory(),
    routes,
  }
);
console.log(router)
// 注册导航守卫
guards.before.forEach(router.beforeEach);
guards.after.forEach(router.afterEach);

export default router;
