<template>
<span>
    <div class="songListItem" v-for="song in songList" :key="song.id" :value="song">
      <p>{{song.title}} </p><p> {{song.interpret}} </p><p> {{song.subtitle}}</p>
      <button>Play</button>
      <select name="cars" id="cars">
          <option v-for="diff in song.difficulties" :key="diff">{{diff}}</option>
      </select>
    </div>
</span>
</template>
<script lang="ts">
  import { Options, Vue } from 'vue-class-component';
  import { Song } from '../models/Song';
  @Options({
    props: {
      songList: []
    }
  })
  export default class SongList extends Vue {
    songList!: Song[]
  }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.songListItem  {
    background-color: #ddd;
    min-width: 248px;
    max-width: 248px;
    max-height: 260px;
    border: 1px solid #e7ecf5;
    box-shadow: 0px 8px 10px -5px rgb(1 1 1);
    margin-bottom:25px;
    padding-bottom:3px;
    padding-left:3px;
}

button {
  background-color: #4CAF50; /* Green */
  border: 0px;
  color: white;
  padding: 5px 20px;
  text-align: center;
  text-decoration: none;
  display: inline-block;
  font-size: 16px;
  transition-duration: 0.4s;
}

button:hover {
  background-color: #090; /* Green */
  color: white;
}

select
{
  background-color: #669; /* Green */
  border: 0px;
  color: white;
  margin-left:5px;
  padding: 5px 15px;
  padding-top: -1px;
}
</style>
