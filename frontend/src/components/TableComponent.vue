<template>
  <div class="table-component">
    <table class="table">
      <thead>
        <tr>
          <th v-for="(header, i) in headers" :key="i">
            {{ header }}
          </th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(row, i) in data" :key="i">
          <td v-for="(header, i) in headers" :key="i">
            <input type="text" v-model="row[header.toLowerCase()]" />
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
import "./TableComponent.scss";
export default {
  name: 'TableComponent',
  props: {
    headers: {
      type: Array,
      required: true
    },
    data: {
      type: Array,
      required: true
    }
  }
}
</script>

<style scoped lang="scss" />
