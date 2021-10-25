import { createApp } from 'vue'
import App from './App.vue'
import gameConnection from "../gameConnection";

gameConnection()
createApp(App).mount('#app')