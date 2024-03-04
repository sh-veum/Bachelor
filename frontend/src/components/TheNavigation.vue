<script setup lang="ts">
import { useAuth } from '@/lib/useAuth'
import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
  NavigationMenuContent,
  navigationMenuTriggerStyle
} from './ui/navigation-menu'

const { isAdmin } = useAuth()
const { isLoggedIn, logout } = useAuth()

const handleLogout = async () => {
  logout()
}
</script>

<template>
  <NavigationMenu
    class="justify-between bg-zinc-50 dark:bg-zinc-900 sticky top-0 left-0 right-0 z-20 border-b border-zinc-200 dark:border-zinc-600"
  >
    <!-- Left-aligned items -->
    <NavigationMenuList>
      <NavigationMenuItem>
        <NavigationMenuTrigger>REST API</NavigationMenuTrigger>
        <NavigationMenuContent>
          <ul class="grid w-max gap-3 p-4 md:grid-cols-1">
            <NavigationMenuLink
              href="/rest"
              :class="navigationMenuTriggerStyle()"
              class="bg-zinc-50 hover:bg-zinc-200"
            >
              REST API Test
            </NavigationMenuLink>
            <NavigationMenuLink
              v-if="isLoggedIn"
              href="/rest/create-key"
              :class="navigationMenuTriggerStyle()"
              class="bg-zinc-50 hover:bg-zinc-200"
            >
              REST API Keys
            </NavigationMenuLink>
            <NavigationMenuLink
              v-if="isLoggedIn"
              href="/theme-edit"
              :class="navigationMenuTriggerStyle()"
              class="bg-zinc-50 hover:bg-zinc-200"
            >
              Theme edit
            </NavigationMenuLink>
          </ul>
        </NavigationMenuContent>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuTrigger>GraphQL</NavigationMenuTrigger>
        <NavigationMenuContent>
          <ul class="grid w-max gap-3 p-4 md:grid-cols-1">
            <NavigationMenuLink
              href="/graphql"
              :class="navigationMenuTriggerStyle()"
              class="bg-zinc-50 hover:bg-zinc-200"
            >
              GraphQL Test
            </NavigationMenuLink>
            <NavigationMenuLink
              v-if="isLoggedIn"
              href="/graphql/create-key"
              :class="navigationMenuTriggerStyle()"
              class="bg-zinc-50 hover:bg-zinc-200"
            >
              GraphQL Create Key
            </NavigationMenuLink>
          </ul>
        </NavigationMenuContent>
      </NavigationMenuItem>
    </NavigationMenuList>
    <!-- right side -->
    <div class="flex list-none">
      <NavigationMenuItem v-if="isAdmin">
        <NavigationMenuLink
          href="/admin"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50 hover:bg-zinc-200"
        >
          Admin Page
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-if="isLoggedIn">
        <NavigationMenuLink
          href="#"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50 hover:bg-zinc-200"
          @click.prevent="handleLogout"
        >
          Logout
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-else>
        <NavigationMenuLink
          href="/login"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50 hover:bg-zinc-200"
        >
          Login
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-if="!isLoggedIn">
        <NavigationMenuLink
          href="/register"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50 hover:bg-zinc-200"
        >
          Register
        </NavigationMenuLink>
      </NavigationMenuItem>
    </div>
  </NavigationMenu>
</template>
