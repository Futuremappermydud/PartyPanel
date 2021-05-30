<template>
  <div>
    <h1>SongList</h1>
    <SongList :songList="songList"/>
  </div>
</template>

<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import SongList from './components/SongList.vue';
import { Song } from './models/Song';

@Options({
  components: {
    SongList,
  },
})
export default class App extends Vue {
  fetchSonglist(): Song[] {
    return [
      {
        id: 'd7hsb',
        interpret: "Green Day",
        title: "Holyday",
        subtitle: "feat. Michael Jackson",
        mapper: "FuturMapper",
        difficulties: [
          "Easy",
          "Normal",
          "Hard",
          "Expert",
          "ExpertPlus"
        ]
      },
      {
        id: '9hszd',
        interpret: "Rise Against",
        title: "Help is on the way",
        subtitle: "(explicit)",
        mapper: "FuturMapper 2",
        difficulties: [
          "Easy",
          "Normal",
          "Hard",
          "Expert"
        ]
      }
    ]
  }
  private songList: Song[] = [];

  async mounted() {
    this.songList = await this.fetchSonglist()
  }
}
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}
</style>
