<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { toTypedSchema } from '@vee-validate/zod'
import { useForm } from 'vee-validate'
import * as z from 'zod'
import axios from 'axios';

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
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'

import { FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/components/ui/form'

import { Check, ChevronsUpDown } from 'lucide-vue-next'

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

interface Theme {
  id: string
  themeName: string
  accessibleEndpoints: string[]
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


const formSchema = toTypedSchema(
  z.object({
    name: z
      .string({
        required_error: 'Please enter a name.'
      })
      .min(1, 'Please enter a name.'),
    themes: z.array(
      z.object({
        id: z.string(),
        themeName: z.string(),
        accessibleEndpoints: z.array(z.string())
      })
    ).min(1, 'Please select at least one theme.')
  })
)

const { handleSubmit, setValues, values } = useForm({
  validationSchema: formSchema
})

const onSubmit = handleSubmit((values) => {
  createAccessKey(values.name, values.themes);
})

const createAccessKey = async (keyName: string, themes: Theme[]) => {
  console.log('Creating access key:', keyName, themes)
  
  try {
    const response = await axios.post('http://localhost:8088/api/key/create-accesskey', {
      keyName: keyName,
      themes: themes,
    });
    console.log('Access key created:',  response.data)
    keys.value.push(response.data);
  } catch (error) {
    console.error('Error creating access key:', (error as any).response.data);
    // Handle error
  }
}

const deleteAccessKey = async (encryptedKey: Key) => {
  console.log('Not yet implemented');
  // try {
  //   await axios.post('http://localhost:8088/api/key/delete-accesskey', { EncryptedKey: encryptedKey.id });
  //   console.log('Access key deleted');
  //   keys.value = keys.value.filter((key) => key.id !== encryptedKey.id);
  // } 
  // catch (error) {
  //   console.error('Error deleting access key:', (error as any).response.data);
  //   // Handle error
  // }
};



const fetchKeys = async () => {
  try {
    //TODO: should the url be more dynamic?
    const keysResponse = await axios.get('http://localhost:8088/api/key/get-apikeys-by-user?type=rest')
    keys.value = keysResponse.data
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const fetchThemes = async () => {
  try {
    //TODO: should the url be more dynamic?
    const themesResponse = await axios.get('http://localhost:8088/api/key/get-themes-by-user')
    themes.value = themesResponse.data
  } catch (error) {
    console.error('Failed to fetch data:', error)
  }
}

const fetchData = async () => {
  await fetchKeys()
  await fetchThemes()
}

onMounted(fetchData)

</script>

<template>
  <div class="flex justify-between">
    <h1 class="text-3xl font-semibold mb-8 px-2">Your API keys</h1>
    <Dialog>
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
                                ? themes.find((t) => t.id === values.themes?.[0].id)?.themeName
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
                          <CommandItem
                            v-for="theme in themes"
                            :key="theme.id"
                            :value="theme"
                            @select="
                              () => {
                                const selectedThemes = values.themes?.includes(theme)
                                  ? values.themes.filter((t) => t.id !== theme.id)
                                  : [...(values.themes ?? []), theme]
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
                                  values.themes?.includes(theme) ? 'opacity-100' : 'opacity-0'
                                )
                              "
                            />
                            {{ theme.themeName }}
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
        <TableHead class="w-[200px] text-center">Actions</TableHead>
        <TableHead>Expires in (days)</TableHead>
      </TableRow>
    </TableHeader>
    <TableBody>
      <TableRow v-for="(key, index) in keys" :key="index">
        <TableCell>{{ key.keyName }}</TableCell>
        <TableCell>
          <ThemeCollapsible v-if="key.themes.length > 1" :apiKey="key" />
          <div v-else class="py-3 font-mono text-sm">
            {{ key.themes[0].themeName }}
          </div>
        </TableCell>
        <TableCell class="text-center"
          ><Button variant="destructive" @click="deleteAccessKey(key)"> Delete </Button></TableCell
        >
        <TableCell>{{ key.expiresIn }}</TableCell>
      </TableRow>
    </TableBody>
  </Table>
</template>
