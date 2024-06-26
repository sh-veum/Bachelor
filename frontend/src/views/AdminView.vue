<script setup lang="ts">
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table'
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogTrigger
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { ref, onMounted } from 'vue'
import axios from 'axios'

interface User {
  userName: string
  email: string
  assignedDatabase: string | null
  role: string | null
  isEditing: boolean
}

// New interfaces for dropdowns
interface EditableUser extends User {
  newRole?: string
  newDatabase?: string
}

const users = ref<EditableUser[]>([])
const databases = ref<string[]>([])
const roles = ref<string[]>([])

// Fetch all users, databases, and roles
const fetchData = async () => {
  try {
    const usersResponse = await axios.get(
      `${import.meta.env.VITE_VUE_APP_API_URL}/api/user/get-all-users`
    )
    users.value = usersResponse.data.map((user: User) => ({ ...user, isEditing: false }))

    const databasesResponse = await axios.get(
      `${import.meta.env.VITE_VUE_APP_API_URL}/api/database/get-database-names`
    )
    databases.value = databasesResponse.data

    const rolesResponse = await axios.get(`${import.meta.env.VITE_VUE_APP_API_URL}/api/user/roles`)
    roles.value = rolesResponse.data.map((role: any) => role.name)
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const editUser = (user: EditableUser) => {
  user.isEditing = true
  user.newDatabase = user.assignedDatabase ?? undefined
  user.newRole = user.role ?? undefined
}

const saveChanges = async (user: EditableUser) => {
  try {
    console.log('Saving changes for user:', user)
    console.log('newRole:', user.newRole)
    console.log('role:', user.role)
    if (user.newRole !== user.role) {
      await axios.post(`${import.meta.env.VITE_VUE_APP_API_URL}/api/user/change-role`, {
        email: user.email,
        role: user.newRole
      })
    }
    if (user.newDatabase !== user.assignedDatabase) {
      await axios.post(`${import.meta.env.VITE_VUE_APP_API_URL}/api/user/update-database-name`, {
        email: user.email,
        database: user.newDatabase
      })
    }
    user.assignedDatabase = user.newDatabase ?? null
    user.role = user.newRole ?? null
    user.isEditing = false
    // fetchData()
  } catch (error) {
    console.error('Failed to save changes:', error)
  }
}

const deleteUser = async (userEmail: string) => {
  try {
    await axios.delete(
      `${import.meta.env.VITE_VUE_APP_API_URL}/api/user/delete-user-by-email?email=${userEmail}`
    )
    fetchData()
  } catch (error) {
    console.error('Failed to delete user:', error)
  }
}

onMounted(fetchData)
</script>

<template>
  <div class="mx-6">
    <Table class="w-[800px]">
      <TableCaption>List of All Users</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead class="w-[200px]">Email</TableHead>
          <TableHead class="w-[200px]">Database</TableHead>
          <TableHead class="w-[200px]">Role</TableHead>
          <TableHead class="w-[200px]">Edit</TableHead>
          <TableHead class="w-[200px]">Delete</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow v-for="user in users" :key="user.email" class="h-20">
          <TableCell>{{ user.email }}</TableCell>
          <TableCell>
            <Select v-if="user.isEditing" v-model="user.newDatabase">
              <SelectTrigger>
                <SelectValue :value="user.newDatabase" placeholder="Select a database" />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>Databases</SelectLabel>
                  <SelectItem v-for="db in databases" :key="db" :value="db">
                    {{ db }}
                  </SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span v-else>{{ user.assignedDatabase || 'N/A' }}</span>
          </TableCell>
          <TableCell>
            <Select v-if="user.isEditing" v-model="user.newRole">
              <SelectTrigger>
                <SelectValue :value="user.newRole" placeholder="Select a role" />
              </SelectTrigger>
              <SelectContent>
                <SelectGroup>
                  <SelectLabel>Roles</SelectLabel>
                  <SelectItem v-for="role in roles" :key="role" :value="role">
                    {{ role }}
                  </SelectItem>
                </SelectGroup>
              </SelectContent>
            </Select>
            <span v-else>{{ user.role || 'Unassigned' }}</span>
          </TableCell>
          <TableCell>
            <Button
              v-if="user.isEditing"
              @click="saveChanges(user)"
              class="p-2 bg-blue-500 text-white rounded"
            >
              Save
            </Button>
            <Button v-else @click="editUser(user)" class="p-2 bg-green-500 text-white rounded">
              Edit
            </Button>
          </TableCell>
          <TableCell>
            <Dialog>
              <DialogTrigger as-child>
                <Button
                  variant="destructive"
                  class="p-2 hover:bg-red-800"
                  :disabled="user.role === 'ADMIN'"
                >
                  Delete
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Delete User</DialogTitle>
                  <DialogDescription>
                    Are you sure you want to delete this user? This action cannot be undone.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button
                    @click="deleteUser(user.email)"
                    variant="destructive"
                    class="p-2 hover:bg-red-800"
                  >
                    Confirm Delete
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
