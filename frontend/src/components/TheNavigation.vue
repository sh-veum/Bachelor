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
import NavigationListItem from './NavigationListItem.vue'
import Separator from './ui/separator/Separator.vue'

const { isAdmin } = useAuth()
const { isLoggedIn, logout } = useAuth()

const handleLogout = async () => {
  logout()
}
</script>

<template>
  <div>
    <NavigationMenu class="justify-between p-1 bg-background">
      <!-- Left-aligned items -->
      <NavigationMenuList>
        <NavigationMenuItem v-if="isLoggedIn">
          <NavigationMenuTrigger>REST API</NavigationMenuTrigger>
          <NavigationMenuContent>
            <ul class="grid gap-3 p-4 w-max md:grid-cols-2">
              <NavigationListItem href="/rest" title="REST API test">
                Test an accesskey
              </NavigationListItem>
              <NavigationListItem v-if="isLoggedIn" href="/rest/create-key" title="REST API keys">
                Create, edit and delete your accesskeys
              </NavigationListItem>
              <NavigationListItem v-if="isLoggedIn" href="/theme-edit" title="Theme edit">
                Sort endpoints into themes for accesskeys
              </NavigationListItem>
            </ul>
          </NavigationMenuContent>
        </NavigationMenuItem>
        <NavigationListItem v-else href="/rest" title="REST API test">
          <!-- Test an accesskey using REST API -->
        </NavigationListItem>
        <NavigationMenuItem v-if="isLoggedIn">
          <NavigationMenuTrigger>GraphQL</NavigationMenuTrigger>
          <NavigationMenuContent>
            <ul class="grid gap-3 p-4 w-max md:grid-cols-2">
              <NavigationListItem href="/graphql" title="GraphQL Test">
                Test an accesskey
              </NavigationListItem>
              <NavigationListItem
                v-if="isLoggedIn"
                href="/graphql/create-key"
                title="GraphQL Create Key"
              >
                Create, edit and delete your accesskeys
              </NavigationListItem>
            </ul>
          </NavigationMenuContent>
        </NavigationMenuItem>
        <NavigationListItem v-else href="/graphql" title="GraphQL Test">
          <!-- Test an accesskey using GraphQL -->
        </NavigationListItem>
      </NavigationMenuList>
      <!-- right side -->
      <div class="flex list-none">
        <NavigationMenuItem v-if="isAdmin">
          <NavigationMenuLink href="/admin" :class="navigationMenuTriggerStyle()">
            Admin Page
          </NavigationMenuLink>
        </NavigationMenuItem>
        <NavigationMenuItem v-if="isLoggedIn">
          <NavigationMenuLink
            href="#"
            :class="navigationMenuTriggerStyle()"
            @click.prevent="handleLogout"
          >
            Logout
          </NavigationMenuLink>
        </NavigationMenuItem>
        <NavigationMenuItem v-else>
          <NavigationMenuLink href="/login" :class="navigationMenuTriggerStyle()">
            Login
          </NavigationMenuLink>
        </NavigationMenuItem>
        <NavigationMenuItem v-if="!isLoggedIn">
          <NavigationMenuLink href="/register" :class="navigationMenuTriggerStyle()">
            Register
          </NavigationMenuLink>
        </NavigationMenuItem>
      </div>
    </NavigationMenu>
    <Separator />
  </div>
</template>
