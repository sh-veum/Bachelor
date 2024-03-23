import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '@/lib/useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // Redirect to the rest API test view
    {
      path: '/',
      redirect: '/rest'
    },
    {
      path: '/rest',
      name: 'rest API Test',
      component: () => import('../views/RestTestView.vue')
    },
    {
      path: '/rest/create-key',
      name: 'rest api create key',
      component: () => import('../views/KeysView.vue'),
      beforeEnter: (to, from, next) => {
        const { isLoggedIn } = useAuth()
        if (!isLoggedIn.value) {
          next('/login')
        } else {
          next()
        }
      }
    },
    {
      path: '/theme',
      name: 'theme',
      component: () => import('../views/ThemeView.vue'),
      beforeEnter: (to, from, next) => {
        const { isLoggedIn } = useAuth()
        if (!isLoggedIn.value) {
          next('/login')
        } else {
          next()
        }
      }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue')
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue')
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('../views/AdminView.vue'),
      beforeEnter: (to, from, next) => {
        const { isAdmin } = useAuth()
        if (!isAdmin.value) {
          next('/login')
        } else {
          next()
        }
      }
    },
    {
      path: '/graphql',
      name: 'GraphQL Key Test',
      component: () => import('../views/GraphQLTestView.vue')
    },
    {
      path: '/graphql/create-key',
      name: 'GraphQL Create Key',
      component: () => import('../views/GraphQLCreateKeyView.vue'),
      beforeEnter: (to, from, next) => {
        const { isLoggedIn } = useAuth()
        if (!isLoggedIn.value) {
          next('/login')
        } else {
          next()
        }
      }
    },
    {
      path: '/kafka',
      name: 'Kafka Test',
      component: () => import('../views/KafkaTestView.vue')
    },
    {
      path: '/kafka/create-key',
      name: 'Kafka Create Key',
      component: () => import('../views/KafkaCreateKeyView.vue'),
      beforeEnter: (to, from, next) => {
        const { isLoggedIn } = useAuth()
        if (!isLoggedIn.value) {
          next('/login')
        } else {
          next()
        }
      }
    }
  ]
})

export default router
