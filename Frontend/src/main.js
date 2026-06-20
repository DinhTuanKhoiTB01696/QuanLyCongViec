import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import './assets/styles/sprinta-entity.scss'
import './utils/theme.js'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router from './router'
import vue3GoogleLogin from 'vue3-google-login'
import VueApexCharts from 'vue3-apexcharts'

const app = createApp(App)

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

app.use(createPinia())
app.use(ElementPlus)
app.use(router)
app.use(VueApexCharts)
app.use(vue3GoogleLogin, {
  clientId: '1008910270642-b5ic5oo3sb2rnemts5dp9sfaq025cud8.apps.googleusercontent.com'
})
app.mount('#app')
