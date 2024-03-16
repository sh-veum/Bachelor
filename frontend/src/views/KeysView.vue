<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'
import axios from 'axios'

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
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'

import { FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'

import { Check, ChevronsUpDown, Copy } from 'lucide-vue-next'

import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList
} from '@/components/ui/command'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { cn } from '@/lib/utils'
import ThemeCollapsible from '@/components/ThemeCollapsible.vue'
import { ref, onMounted } from 'vue'
import Label from '@/components/ui/label/Label.vue'

interface Theme {
  id: string
  themeName: string
  accessibleEndpoints: string[]
  isDeprecated: boolean
}

interface Key {
  id: string
  keyName: string
  createdBy: string
  expiresIn: number
  isEnabled: boolean
  themes: Theme[]
}

const keys = ref<Key[]>([])
const themes = ref<Theme[]>([])
const createIsOpen = ref(false)
const copyIsOpen = ref(false)
const encryptedKey = ref('something')
const copySuccess = ref(false)

const formSchema = toTypedSchema(
  z.object({
    name: z
      .string({
        required_error: 'Please enter a name.'
      })
      .min(1, 'Please enter a name.'),
    themes: z.array(z.string()).min(1, 'Please select at least one theme.')
  })
)

const { handleSubmit, setValues, values } = useForm({
  validationSchema: formSchema
})

const onSubmit = handleSubmit((values) => {
  const selectedThemes = themes.value.filter((theme) => values.themes.includes(theme.id))
  createAccessKey(values.name, selectedThemes).then(() => {
    fetchKeys()
  })
  createIsOpen.value = false
})

const createAccessKey = async (keyName: string, themes: Theme[]) => {
  console.log('Creating access key:', keyName, themes)

  try {
    const response = await axios.post('http://localhost:8088/api/rest/create-accesskey', {
      keyName: keyName,
      themeIds: themes.map((theme) => theme.id)
    })
    console.log('Access key created:', response.data)
    encryptedKey.value = response.data.encryptedKey
    copyIsOpen.value = true
  } catch (error) {
    console.error('Error creating access key:', error)
    // TODO: handle error
  }
}

const deleteAccessKey = async (id: string) => {
  try {
    await axios
      .delete(`http://localhost:8088/api/rest/delete-accesskey?id=${id}`, {})
      .then((response) => {
        // update the list of keys in frontend
        console.log('Key deleted:', response)
        const keyIndex = keys.value.findIndex((key) => key.id === id)
        if (keyIndex !== -1) {
          keys.value.splice(keyIndex, 1)
        }
      })
  } catch (error) {
    console.error('Failed to delete key:', error)
  }
}

const fetchKeys = async () => {
  try {
    //TODO: should the url be more dynamic?
    const keysResponse = await axios.get('http://localhost:8088/api/rest/get-keys-by-user')
    keys.value = keysResponse.data
  } catch (error) {
    console.error('Failed to fetch keys:', error)
  }
}

const fetchThemes = async () => {
  try {
    //TODO: should the url be more dynamic?
    const themesResponse = await axios.get('http://localhost:8088/api/rest/get-themes-by-user')
    themes.value = themesResponse.data
  } catch (error) {
    console.error('Failed to fetch themes:', error)
  }
}

const toggleKeyEnabledStatus = async (id: string, isEnabled: boolean) => {
  try {
    await axios.patch('http://localhost:8088/api/rest/toggle-apikey', {
      id: id,
      keyType: 'rest',
      isEnabled: isEnabled
    })
    const keyIndex = keys.value.findIndex((key) => key.id === id)
    if (keyIndex !== -1) {
      keys.value[keyIndex].isEnabled = isEnabled
    }
  } catch (error) {
    console.error('Failed to toggle key:', error)
  }
}

const fetchData = async () => {
  await fetchKeys()
  await fetchThemes()
}

const copyLink = () => {
  navigator.clipboard.writeText(encryptedKey.value)
  copySuccess.value = true
  setTimeout(() => {
    copySuccess.value = false
  }, 1000)
}

onMounted(() => {
  fetchData()
})
</script>

<template>
  <Dialog v-model:open="copyIsOpen">
    <!-- TODO: remove this dialog trigger (its only here for testing purposes) -->
    <DialogTrigger as-child>
      <Button variant="outline"> Test key copy </Button>
    </DialogTrigger>
    <!-- Prevent user from closing dialog by clicking outside of it -->
    <DialogContent
      @interact-outside="
        (event) => {
          return event.preventDefault()
        }
      "
      class="sm:max-w-md"
    >
      <DialogHeader>
        <DialogTitle class="text-center">Key created successfully!</DialogTitle>
        <DialogDescription class="text-red-500 font-bold italic text-center">
          Store the key, you won't be able to see it again when you close the dialog
        </DialogDescription>
      </DialogHeader>
      <div class="flex items-center space-x-2">
        <div class="grid flex-1 gap-2">
          <Label for="link" class="sr-only"> Link </Label>
          <Input id="link" :model-value="encryptedKey" readonly />
        </div>
        <Button type="submit" size="sm" class="px-3">
          <span class="sr-only">Copy</span>
          <div v-if="copySuccess">
            <Check class="w-4 h-4 text-green-500" />
          </div>
          <div v-else>
            <Copy @click="copyLink" class="w-4 h-4" />
          </div>
        </Button>
      </div>
      <DialogFooter class="sm:justify-start">
        <DialogClose as-child>
          <Button type="button" variant="secondary"> Close </Button>
        </DialogClose>
      </DialogFooter>
    </DialogContent>
  </Dialog>

  <div class="flex justify-between">
    <h1 class="text-3xl font-semibold mb-8 px-2">Your API keys</h1>
    <Dialog v-model:open="createIsOpen">
      <DialogTrigger as-child>
        <Button class="mr-4"> Create new key </Button>
      </DialogTrigger>
      <DialogContent class="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Create new key</DialogTitle>
          <DialogDescription>
            Choose a name for the key and which themes the key can access.
          </DialogDescription>
        </DialogHeader>
        <form class="space-y-6" @submit="onSubmit">
          <div class="grid gap-4 py-4">
            <FormField v-slot="{ componentField }" name="name">
              <FormItem>
                <FormLabel>Name</FormLabel>
                <FormControl>
                  <Input type="text" placeholder="Name" v-bind="componentField" />
                </FormControl>
                <FormMessage />
              </FormItem>
            </FormField>

            <FormField name="themes">
              <FormItem class="flex flex-col">
                <FormLabel>Themes</FormLabel>
                <Popover>
                  <PopoverTrigger as-child>
                    <FormControl>
                      <Button
                        variant="outline"
                        role="combobox"
                        :class="
                          cn(
                            'w-[200px] justify-between',
                            !values.themes?.length && 'text-muted-foreground'
                          )
                        "
                      >
                        {{
                          values.themes?.length
                            ? values.themes.length === 1
                              ? themes.find((t) => t.id === values.themes?.[0])?.themeName
                              : `${values.themes.length} themes selected`
                            : 'Select themes...'
                        }}
                        <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                      </Button>
                    </FormControl>
                  </PopoverTrigger>
                  <PopoverContent class="w-[200px] p-0">
                    <Command multiple>
                      <CommandInput placeholder="Search themes..." />
                      <CommandEmpty>Nothing found.</CommandEmpty>
                      <CommandList>
                        <CommandGroup>
                          <!-- To remove themes from the select all together: -->
                          <!-- v-for="theme in themes.filter((t) => !t.isDeprecated)" -->
                          <CommandItem
                            v-for="theme in themes"
                            :key="theme.id"
                            :value="theme.id"
                            :disabled="theme.isDeprecated"
                            @select="
                              () => {
                                const selectedThemes = values.themes?.includes(theme.id)
                                  ? values.themes.filter((t) => t !== theme.id)
                                  : [...(values.themes ?? []), theme.id]
                                setValues({
                                  themes: selectedThemes
                                })
                                console.log('Selected themes:', selectedThemes)
                              }
                            "
                          >
                            <Check
                              :class="
                                cn(
                                  'mr-2 h-4 w-4',
                                  values.themes?.includes(theme.id) ? 'opacity-100' : 'opacity-0'
                                )
                              "
                            />
                            {{ theme.themeName }}
                            <span v-if="theme.isDeprecated" class="text-red-500">
                              (deprecated)</span
                            >
                          </CommandItem>
                        </CommandGroup>
                      </CommandList>
                    </Command>
                  </PopoverContent>
                </Popover>
                <FormMessage />
              </FormItem>
            </FormField>
          </div>
          <DialogFooter>
            <Button type="submit"> Create </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  </div>

  <Table>
    <TableCaption>A list of your created keys</TableCaption>
    <TableHeader>
      <TableRow>
        <TableHead class="w-[200px]">Name</TableHead>
        <TableHead>Themes</TableHead>
        <TableHead>Expires in (days)</TableHead>
        <TableHead class="w-[200px] text-center">Disable</TableHead>
        <TableHead class="w-[200px] text-center">Delete</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="key in keys" :key="key.id">
        <TableCell>{{ key.keyName }}</TableCell>
        <TableCell>
          <ThemeCollapsible v-if="key.themes.length > 1" :apiKey="key" />
          <div v-else class="py-3 font-mono text-sm">
            {{ key.themes[0]?.themeName ?? 'No themes' }}
          </div>
        </TableCell>
        <TableCell>{{ key.expiresIn }}</TableCell>
        <TableCell class="text-center">
          <Button
            class="bg-zinc-600"
            v-if="key.isEnabled"
            @click="toggleKeyEnabledStatus(key.id, false)"
          >
            Disable
          </Button>
          <Button class="bg-green-600" v-else @click="toggleKeyEnabledStatus(key.id, true)">
            Enable
          </Button>
        </TableCell>
        <TableCell class="text-center"
          ><Button variant="destructive" @click="deleteAccessKey(key.id)">
            Delete
          </Button></TableCell
        >
      </TableRow>
    </TableBody>
  </Table>
</template>
