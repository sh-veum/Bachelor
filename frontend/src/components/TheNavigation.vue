<script setup lang="ts">
import { useAuth } from '@/lib/useAuth'
import {
  NavigationMenu,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  navigationMenuTriggerStyle
} from './ui/navigation-menu'

const { isAdmin } = useAuth()
const { isLoggedIn } = useAuth()
</script>

<template>
  <NavigationMenu
    class="justify-between bg-zinc-50 dark:bg-zinc-900 sticky top-0 left-0 right-0 z-20 border-b border-zinc-200 dark:border-zinc-600"
  >
    <!-- Left-aligned items -->
    <NavigationMenuList>
      <NavigationMenuItem v-if="isLoggedIn">
        <NavigationMenuLink href="/" :class="navigationMenuTriggerStyle()" class="bg-zinc-50">
          Keys
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-if="isLoggedIn">
        <NavigationMenuLink
          href="/theme-edit"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50"
        >
          Theme edit
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink
          href="/customer"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50"
        >
          REST API Test
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-if="isAdmin">
        <NavigationMenuLink href="/admin" :class="navigationMenuTriggerStyle()" class="bg-zinc-50">
          Admin Page
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink
          href="/graphql"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50"
        >
          GraphQL Test
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-if="isLoggedIn">
        <NavigationMenuLink
          href="/graphql/create-key"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50"
        >
          GraphQL Create Key
        </NavigationMenuLink>
      </NavigationMenuItem>
    </NavigationMenuList>
    <!-- right side -->
    <div class="flex list-none">
      <NavigationMenuItem>
        <NavigationMenuLink href="/login" :class="navigationMenuTriggerStyle()" class="bg-zinc-50">
          Login
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink
          href="/register"
          :class="navigationMenuTriggerStyle()"
          class="bg-zinc-50"
        >
          Register
        </NavigationMenuLink>
      </NavigationMenuItem>
    </div>
  </NavigationMenu>
</template>
