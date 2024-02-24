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
</script>

<template>
  <NavigationMenu>
    <NavigationMenuList>
      <NavigationMenuItem>
        <NavigationMenuLink href="/" :class="navigationMenuTriggerStyle()">
          Keys
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink href="/theme-edit" :class="navigationMenuTriggerStyle()">
          Theme edit
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink href="/customer" :class="navigationMenuTriggerStyle()">
          Customer page
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink href="/login" :class="navigationMenuTriggerStyle()">
          Login
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink href="/register" :class="navigationMenuTriggerStyle()">
          Register
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem v-if="isAdmin">
        <NavigationMenuLink href="/admin" :class="navigationMenuTriggerStyle()">
          Admin Page
        </NavigationMenuLink>
      </NavigationMenuItem>
      <NavigationMenuItem>
        <NavigationMenuLink href="/graphql" :class="navigationMenuTriggerStyle()">
          GraphQL
        </NavigationMenuLink>
      </NavigationMenuItem>
    </NavigationMenuList>
  </NavigationMenu>
</template>
