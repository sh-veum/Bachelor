import { createRouter, createWebHistory } from 'vue-router'
import KeysView from '../views/KeysView.vue'
import { useAuth } from '@/lib/useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'keys',
      component: KeysView
    },
    {
      path: '/theme-edit',
      name: 'theme-edit',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/ThemeEditView.vue')
    },
    {
      path: '/customer',
      name: 'customer',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/CustomerView.vue')
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
      component: () => import('../views/GraphQLCreateKeyView.vue')
    }
  ]
})

export default router
